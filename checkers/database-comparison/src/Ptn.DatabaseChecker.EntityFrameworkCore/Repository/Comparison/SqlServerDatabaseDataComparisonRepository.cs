using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.SqlServer;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Capabilities;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Projections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Projections;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;
using Volo.Abp.DependencyInjection;
using EfHistory = Ptn.DatabaseChecker.Constants.Comparison.DatabaseDataComparisonConstants.EntityFrameworkMigrationsHistory;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: SQL Server hedeflerinde EF migration history'yi, tablo/PK yapisini, kesin sayimi ve exact JSON satir verisini batch okur.
// sistemdeki gorevi: IDatabaseDataComparisonRepository'nin SqlServer implementasyonu; ortak akisi base'ten alir, yalniz sys katalog context'ini, identifier quoting'ini, sayim ifadesini ve sys.objects/sys.schemas uzerinden varlik kontrolunu verir.
[ExposeServices(
    typeof(IDatabaseDataComparisonRepository),
    typeof(IProjectionRepository),
    typeof(IWriteSetRepository))]
public class SqlServerDatabaseDataComparisonRepository
    : DatabaseDataComparisonRepositoryBase, ITransientDependency
{
    private const int PermissionDeniedErrorNumber = 229;

    public override string EngineCode => DatabaseEngineCodes.SqlServer;

    // SQL Server EF migration history tablosunun desteklenen varsayilan semasi.
    protected override string MigrationHistorySchemaName => EfHistory.SqlServerDefaultSchema;

    // SQL Server kesin satir sayimi; count_big(*) bigint doner, TableRowCountModel.RowCount (long) ile hizalanir.
    protected override string CountExpression => "count_big(*)";

    // SQL Server row alias'ini tum kolonlari ve null degerleri iceren tek JSON nesnesine cevirir.
    protected override string BuildRowJsonExpression(string rowAlias)
        => $"(select {rowAlias}.* for json path, without_array_wrapper, include_null_values)";

    // islevi: Secili SQL Server kolonlarini yalniz identifier quoting kullanarak tek JSON nesnesine cevirir.
    protected override string BuildProjectedRowJsonExpression(
        string rowAlias,
        IReadOnlyList<string> projectColumns,
        List<object> parameters)
    {
        var selection = string.Join(", ", projectColumns.Select(column =>
            $"{rowAlias}.{QuoteIdentifier(column)}"));
        return $"(select {selection} for json path, without_array_wrapper, include_null_values)";
    }

    // islevi: SQL Server SELECT yetki reddini provider detayini sizdirmadan kapali projection yetki koduna cevirir.
    public override async Task<List<ProjectionRow>> ReadProjectionRowsAsync(
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        List<string> projectColumns,
        int maxRows,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.ReadProjectionRowsAsync(
                info, structure, keyValues, projectColumns, maxRows, cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == PermissionDeniedErrorNumber)
        {
            throw new Volo.Abp.BusinessException(
                AssertionExceptionCodes.ProjectionNotAuthorized,
                innerException: exception);
        }
    }

    // islevi: Hedef SQL Server'a baglanan, sema kesfi + __EFMigrationsHistory mapli katalog context'ini kurar.
    protected override DbContext CreateCatalogContext(DatabaseConnectionInfo info)
        => SqlServerCatalogDbContext.Create(info);

    // islevi: dbo.__EFMigrationsHistory tablosunun varligini sys.objects + sys.schemas uzerinden LINQ ile kontrol eder.
    protected override Task<bool> MigrationHistoryExistsAsync(
        DbContext catalogContext,
        CancellationToken cancellationToken)
    {
        var context = (SqlServerCatalogDbContext)catalogContext;
        return (
            from schemaObject in context.Set<SqlServerObjectCatalogRow>()
            join schema in context.Set<SqlServerSchemaCatalogRow>()
                on schemaObject.SchemaId equals schema.Id
            where schema.Name == MigrationHistorySchemaName
                  && schemaObject.Name == EfHistory.TableName
            select schemaObject.Id)
            .AnyAsync(cancellationToken);
    }

    // islevi: Istenen SQL Server tablolarinin mevcut adres, kolon ve PK yapisini sys kataloglarindan batch okur.
    protected override async Task<List<TableDataStructureModel>> ReadTableStructuresCoreAsync(
        DbContext catalogContext,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken)
    {
        var context = (SqlServerCatalogDbContext)catalogContext;
        var requestedKeys = tables
            .Select(table => BuildTableKey(table.SchemaName, table.TableName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var schemaNames = tables.Select(table => table.SchemaName).Distinct().ToList();
        var tableNames = tables.Select(table => table.TableName).Distinct().ToList();

        var schemas = await context.Set<SqlServerSchemaCatalogRow>()
            .Where(schema => schemaNames.Contains(schema.Name))
            .ToDictionaryAsync(schema => schema.Id, schema => schema.Name, cancellationToken);
        var schemaIds = schemas.Keys.ToList();
        var catalogTables = await context.Set<SqlServerObjectCatalogRow>()
            .Where(table => schemaIds.Contains(table.SchemaId) &&
                            tableNames.Contains(table.Name) &&
                            table.Type == DatabaseMetadataCatalogConstants.SqlServer.UserTableObjectType &&
                            !table.IsMsShipped)
            .ToListAsync(cancellationToken);
        catalogTables = catalogTables
            .Where(table => requestedKeys.Contains(BuildTableKey(schemas[table.SchemaId], table.Name)))
            .ToList();

        var tableIds = catalogTables.Select(table => table.Id).ToList();
        var columns = await ReadColumnsAsync(context, tableIds, cancellationToken);
        var primaryKeyColumns = await ReadPrimaryKeyColumnsAsync(context, tableIds, cancellationToken);
        return BuildStructures(catalogTables, schemas, columns, primaryKeyColumns);
    }

    // islevi: Secili SQL Server tablolarinin kullanici kolonlarini tek katalog sorgusuyla okur.
    private static Task<List<SqlServerColumnCatalogRow>> ReadColumnsAsync(
        SqlServerCatalogDbContext context,
        List<int> tableIds,
        CancellationToken cancellationToken)
        => context.Set<SqlServerColumnCatalogRow>()
            .Where(column => tableIds.Contains(column.ObjectId))
            .OrderBy(column => column.ObjectId)
            .ThenBy(column => column.ColumnId)
            .ToListAsync(cancellationToken);

    // islevi: Secili SQL Server tablolarinin primary-key kolonlarini key sirasiyla tek join sorgusunda okur.
    private static async Task<List<(int TableId, string ColumnName, byte KeyOrdinal)>> ReadPrimaryKeyColumnsAsync(
        SqlServerCatalogDbContext context,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from index in context.Set<SqlServerIndexCatalogRow>()
            join indexColumn in context.Set<SqlServerIndexColumnCatalogRow>()
                on new { index.ObjectId, index.IndexId }
                equals new { indexColumn.ObjectId, indexColumn.IndexId }
            join column in context.Set<SqlServerColumnCatalogRow>()
                on new { indexColumn.ObjectId, indexColumn.ColumnId }
                equals new { column.ObjectId, column.ColumnId }
            where tableIds.Contains(index.ObjectId) &&
                  index.IsPrimaryKey &&
                  indexColumn.KeyOrdinal > 0
            orderby index.ObjectId, indexColumn.KeyOrdinal
            select new
            {
                TableId = index.ObjectId,
                ColumnName = column.Name,
                indexColumn.KeyOrdinal
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => (row.TableId, row.ColumnName, row.KeyOrdinal))
            .ToList();
    }

    // islevi: SQL Server katalog satirlarini tablo bazli provider-notr data structure modellerine gruplar.
    private static List<TableDataStructureModel> BuildStructures(
        List<SqlServerObjectCatalogRow> tables,
        Dictionary<int, string> schemas,
        List<SqlServerColumnCatalogRow> columns,
        List<(int TableId, string ColumnName, byte KeyOrdinal)> primaryKeyColumns)
    {
        var columnsByTable = columns
            .GroupBy(column => column.ObjectId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var primaryKeysByTable = primaryKeyColumns
            .GroupBy(column => column.TableId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(column => column.KeyOrdinal).Select(column => column.ColumnName).ToList());

        return tables
            .Select(table => new TableDataStructureModel
            {
                SchemaName = schemas[table.SchemaId],
                TableName = table.Name,
                ColumnNames = columnsByTable
                    .GetValueOrDefault(table.Id, new List<SqlServerColumnCatalogRow>())
                    .Select(column => column.Name)
                    .ToList(),
                PrimaryKeyColumns = primaryKeysByTable.GetValueOrDefault(table.Id, new List<string>())
            })
            .OrderBy(table => table.SchemaName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(table => table.TableName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // islevi: SQL Server identifier'ini guvenli bracket formuna cevirir.
    protected override string QuoteIdentifier(string identifier)
        => $"{SchemaComparisonTextConstants.Normalization.SqlServerIdentifierOpen}" +
           $"{identifier.Replace(
               DatabaseMetadataCatalogConstants.SqlServer.IdentifierClose,
               DatabaseMetadataCatalogConstants.SqlServer.EscapedIdentifierClose,
               System.StringComparison.Ordinal)}" +
           DatabaseMetadataCatalogConstants.SqlServer.IdentifierClose;
}
