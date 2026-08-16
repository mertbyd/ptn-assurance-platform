using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.Replication;
using Ptn.DatabaseChecker.Connections;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.PostgreSql;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Capabilities;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Projections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Projections;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;
using Volo.Abp.DependencyInjection;
using EfHistory = Ptn.DatabaseChecker.Constants.Comparison.DatabaseDataComparisonConstants.EntityFrameworkMigrationsHistory;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: PostgreSQL hedeflerinde EF migration history'yi, tablo/PK yapisini, kesin sayimi ve exact JSON satir verisini batch okur.
// sistemdeki gorevi: IDatabaseDataComparisonRepository'nin PostgreSql implementasyonu; ortak akisi base'ten alir, yalniz pg katalog context'ini, identifier quoting'ini, sayim ifadesini ve pg_class/pg_namespace uzerinden varlik kontrolunu verir.
[ExposeServices(
    typeof(IDatabaseDataComparisonRepository),
    typeof(IProjectionRepository),
    typeof(IWriteSetRepository))]
public class PostgreSqlDatabaseDataComparisonRepository
    : DatabaseDataComparisonRepositoryBase, ITransientDependency
{
    public override string EngineCode => DatabaseEngineCodes.PostgreSql;

    // islevi: Capability probe sorgulari icin mevcut safety profile ve Vault kimliginden Npgsql baglantisi kurar.
    protected override NpgsqlConnection? CreateWriteSetProbeConnection(DatabaseConnectionInfo info)
        => new(DatabaseConnectionStringFactory.BuildPostgreSql(info));

    // islevi: Temporary logical slot icin ayni safety profile'la Npgsql replication baglantisi kurar.
    protected override LogicalReplicationConnection? CreateWriteSetReplicationConnection(DatabaseConnectionInfo info)
        => new(DatabaseConnectionStringFactory.BuildPostgreSql(info));

    // PostgreSQL EF migration history tablosunun desteklenen varsayilan semasi.
    protected override string MigrationHistorySchemaName => EfHistory.PostgreSqlDefaultSchema;

    // PostgreSQL kesin satir sayimi; count(*) int8 doner, ::bigint ile TableRowCountModel.RowCount (long) ile hizalanir.
    protected override string CountExpression => "count(*)::bigint";

    // PostgreSQL composite row'u kolon adlari + null degerlerle JSON metnine cevirir.
    protected override string BuildRowJsonExpression(string rowAlias)
        => $"to_jsonb({rowAlias})::text";

    // islevi: Secili PostgreSQL kolonlarini JSON anahtarlari parametreli tek nesne ifadesine cevirir.
    protected override string BuildProjectedRowJsonExpression(
        string rowAlias,
        IReadOnlyList<string> projectColumns,
        List<object> parameters)
    {
        var pairs = projectColumns.Select(column =>
            $"{BindParameter(parameters, column)}, {rowAlias}.{QuoteIdentifier(column)}");
        return $"jsonb_build_object({string.Join(", ", pairs)})::text";
    }

    // islevi: PostgreSQL privilege reddini provider detayini sizdirmadan kapali projection yetki koduna cevirir.
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
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.InsufficientPrivilege)
        {
            throw new Volo.Abp.BusinessException(
                AssertionExceptionCodes.ProjectionNotAuthorized,
                innerException: exception);
        }
    }

    // islevi: Hedef PostgreSQL'e baglanan, sema kesfi + __EFMigrationsHistory mapli katalog context'ini kurar.
    protected override DbContext CreateCatalogContext(DatabaseConnectionInfo info)
        => PostgreSqlCatalogDbContext.Create(info);

    // islevi: public.__EFMigrationsHistory tablosunun varligini pg_class + pg_namespace uzerinden LINQ ile kontrol eder.
    protected override Task<bool> MigrationHistoryExistsAsync(
        DbContext catalogContext,
        CancellationToken cancellationToken)
    {
        var context = (PostgreSqlCatalogDbContext)catalogContext;
        return (
            from relation in context.Set<PostgreSqlClassCatalogRow>()
            join schema in context.Set<PostgreSqlNamespaceCatalogRow>()
                on relation.NamespaceId equals schema.Id
            where schema.Name == MigrationHistorySchemaName
                  && relation.Name == EfHistory.TableName
            select relation.Id)
            .AnyAsync(cancellationToken);
    }

    // islevi: Istenen PostgreSQL tablolarinin mevcut adres, kolon ve PK yapisini pg_catalog'dan batch okur.
    protected override async Task<List<TableDataStructureModel>> ReadTableStructuresCoreAsync(
        DbContext catalogContext,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken)
    {
        var context = (PostgreSqlCatalogDbContext)catalogContext;
        var requestedKeys = tables
            .Select(table => BuildTableKey(table.SchemaName, table.TableName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var schemaNames = tables.Select(table => table.SchemaName).Distinct().ToList();
        var tableNames = tables.Select(table => table.TableName).Distinct().ToList();

        var schemas = await context.Set<PostgreSqlNamespaceCatalogRow>()
            .Where(schema => schemaNames.Contains(schema.Name))
            .ToDictionaryAsync(schema => schema.Id, schema => schema.Name, cancellationToken);
        var schemaIds = schemas.Keys.ToList();
        var catalogTables = await context.Set<PostgreSqlClassCatalogRow>()
            .Where(table => schemaIds.Contains(table.NamespaceId) &&
                            tableNames.Contains(table.Name) &&
                            (table.Kind == DatabaseMetadataCatalogConstants.PostgreSql.TableRelKind ||
                             table.Kind == DatabaseMetadataCatalogConstants.PostgreSql.PartitionedTableRelKind))
            .ToListAsync(cancellationToken);
        catalogTables = catalogTables
            .Where(table => requestedKeys.Contains(BuildTableKey(schemas[table.NamespaceId], table.Name)))
            .ToList();

        var tableIds = catalogTables.Select(table => table.Id).ToList();
        var columns = await ReadColumnsAsync(context, tableIds, cancellationToken);
        var primaryKeys = await ReadPrimaryKeysAsync(context, tableIds, cancellationToken);
        return BuildStructures(catalogTables, schemas, columns, primaryKeys);
    }

    // islevi: Secili PostgreSQL tablolarinin kullanici kolonlarini tek katalog sorgusuyla okur.
    private static Task<List<PostgreSqlAttributeCatalogRow>> ReadColumnsAsync(
        PostgreSqlCatalogDbContext context,
        List<uint> tableIds,
        CancellationToken cancellationToken)
        => context.Set<PostgreSqlAttributeCatalogRow>()
            .Where(column => tableIds.Contains(column.RelationId) &&
                             column.ColumnNumber > 0 &&
                             !column.IsDropped)
            .OrderBy(column => column.RelationId)
            .ThenBy(column => column.ColumnNumber)
            .ToListAsync(cancellationToken);

    // islevi: Secili PostgreSQL tablolarinin PK constraint kolon numaralarini tek katalog sorgusuyla okur.
    private static Task<List<PostgreSqlConstraintCatalogRow>> ReadPrimaryKeysAsync(
        PostgreSqlCatalogDbContext context,
        List<uint> tableIds,
        CancellationToken cancellationToken)
        => context.Set<PostgreSqlConstraintCatalogRow>()
            .Where(constraint => tableIds.Contains(constraint.TableRelId) &&
                                 constraint.Type == DatabaseMetadataCatalogConstants.PostgreSql.PrimaryKeyConType)
            .ToListAsync(cancellationToken);

    // islevi: PostgreSQL katalog satirlarini tablo bazli provider-notr data structure modellerine gruplar.
    private static List<TableDataStructureModel> BuildStructures(
        List<PostgreSqlClassCatalogRow> tables,
        Dictionary<uint, string> schemas,
        List<PostgreSqlAttributeCatalogRow> columns,
        List<PostgreSqlConstraintCatalogRow> primaryKeys)
    {
        var columnsByTable = columns
            .GroupBy(column => column.RelationId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var primaryKeyByTable = primaryKeys.ToDictionary(key => key.TableRelId);

        return tables.Select(table =>
        {
            var tableColumns = columnsByTable.GetValueOrDefault(table.Id, new List<PostgreSqlAttributeCatalogRow>());
            var columnNamesByNumber = tableColumns.ToDictionary(column => column.ColumnNumber, column => column.Name);
            var primaryKeyColumns = primaryKeyByTable.TryGetValue(table.Id, out var primaryKey)
                ? (primaryKey.ColumnNumbers ?? Array.Empty<short>())
                    .Where(columnNamesByNumber.ContainsKey)
                    .Select(columnNumber => columnNamesByNumber[columnNumber])
                    .ToList()
                : new List<string>();

            return new TableDataStructureModel
            {
                SchemaName = schemas[table.NamespaceId],
                TableName = table.Name,
                ColumnNames = tableColumns.Select(column => column.Name).ToList(),
                PrimaryKeyColumns = primaryKeyColumns
            };
        })
        .OrderBy(table => table.SchemaName, StringComparer.OrdinalIgnoreCase)
        .ThenBy(table => table.TableName, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    // islevi: PostgreSQL identifier'ini guvenli cift tirnak formuna cevirir.
    protected override string QuoteIdentifier(string identifier)
        => $"{DatabaseMetadataCatalogConstants.PostgreSql.IdentifierQuote}" +
           $"{identifier.Replace(
               DatabaseMetadataCatalogConstants.PostgreSql.IdentifierQuote,
               DatabaseMetadataCatalogConstants.PostgreSql.EscapedIdentifierQuote,
               System.StringComparison.Ordinal)}" +
           DatabaseMetadataCatalogConstants.PostgreSql.IdentifierQuote;
}
