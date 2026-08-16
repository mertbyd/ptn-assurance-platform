using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.SqlServer;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: SQL Server sys kataloglarini EF Core method-syntax LINQ ile okuyup motor-bagimsiz SchemaSnapshotModel'e cevirir.
// sistemdeki gorevi: Bir baglantinin verilen semalarinin tam fotografini (tablo, kolon, default, index, constraint, trigger) uretir; provider'a ozel sys katalog okumalari base snapshot akisina hook olarak baglanir.
public partial class SqlServerDatabaseSchemaDiscoveryRepository
    : DatabaseSchemaDiscoveryRepositoryBase<int>, ITransientDependency
{
    public SqlServerDatabaseSchemaDiscoveryRepository()
    {
    }

    // islevi: SQL Server discovery akisina engine type-map resolver bagimliligini aktarir.
    public SqlServerDatabaseSchemaDiscoveryRepository(
        IEngineComponentResolver<IEngineTypeMapProvider> typeMapProviderResolver)
        : base(typeMapProviderResolver)
    {
    }

    public override string EngineCode => DatabaseEngineCodes.SqlServer;

    // islevi: Domain.Shared'daki vendor sistem-adi kalibini source-generated regex'e cevirir.
    [GeneratedRegex(
        DatabaseMetadataCatalogConstants.SqlServer.SystemGeneratedNamePattern,
        RegexOptions.CultureInvariant)]
    private static partial Regex SqlServerSystemNamePattern();

    // islevi: SQL Server runtime katalog context'ini base snapshot akisina verir.
    protected override DbContext CreateDbContext(DatabaseConnectionInfo info)
        => SqlServerCatalogDbContext.Create(info);

    // islevi: SQL Server veritabani collation adini snapshot basligina tasir; provider kodu desteklenmedigi icin bos kalir.
    protected override async Task PopulateSnapshotMetadataAsync(
        DbContext dbContext,
        SchemaSnapshotModel snapshot,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        snapshot.DatabaseCollationName = await catalogDbContext.Set<SqlServerDatabaseCatalogRow>()
            .Where(database => database.Name == snapshot.DatabaseName)
            .Select(database => database.CollationName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // islevi: Okunacak SQL Server semalarini schema_id -> ad sozlugu olarak dondurur.
    protected override Task<Dictionary<int, string>> GetSchemaNameByIdAsync(
        DbContext dbContext,
        List<string> schemaNames,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        return BuildSchemaQuery(catalogDbContext, schemaNames)
            .ToDictionaryAsync(schema => schema.Id, schema => schema.Name, cancellationToken);
    }

    // islevi: Secilen semalardaki SQL Server kullanici tablolarini aciklamalariyla dondurur.
    protected override Task<List<SchemaDiscoveredTableModel<int>>> GetTablesAsync(
        DbContext dbContext,
        List<int> schemaIds,
        CancellationToken cancellationToken)
        => GetTablesAsync(dbContext, schemaIds, null, cancellationToken);

    protected override async Task<List<SchemaDiscoveredTableModel<int>>> GetTablesAsync(
        DbContext dbContext,
        List<int> schemaIds,
        string? tableName,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var query = BuildTableQuery(catalogDbContext, schemaIds);
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            query = query.Where(dbObject => dbObject.Name == tableName);
        }

        var tables = await query
            .Select(dbObject => new SchemaDiscoveredTableModel<int>
            {
                Id = dbObject.Id,
                SchemaId = dbObject.SchemaId,
                Name = dbObject.Name
            })
            .ToListAsync(cancellationToken);

        return tables;
    }
    // islevi: SQL Server tablo kolonlarini sys.columns + sys.types join'iyle okur ve tablo kimligine gore gruplar.
    protected override async Task<Dictionary<int, List<SchemaColumnModel>>> GetColumnsByTableAsync(
        DbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var defaultByColumn = await ReadDefaultSqlByColumnAsync(catalogDbContext, tableIds, cancellationToken);
        var computedByColumn = await ReadComputedColumnsByColumnAsync(catalogDbContext, tableIds, cancellationToken);
        var identityByColumn = await ReadIdentityValuesByColumnAsync(catalogDbContext, tableIds, cancellationToken);
        var commentsByColumn = await ReadColumnCommentsByColumnAsync(catalogDbContext, tableIds, cancellationToken);
        return await ReadColumnsByTableAsync(
            catalogDbContext,
            tableIds,
            defaultByColumn,
            computedByColumn,
            identityByColumn,
            commentsByColumn,
            cancellationToken);
    }
    // islevi: SQL Server index okuma adimlarini ayri helper'larla yurutur.
    protected override async Task<Dictionary<int, List<SchemaIndexModel>>> GetIndexesByTableAsync(
        DbContext dbContext,
        List<int> tableIds,
        Dictionary<int, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var indexes = await ReadIndexRowsAsync(catalogDbContext, tableIds, cancellationToken);
        var indexColumns = await ReadIndexColumnRowsAsync(catalogDbContext, tableIds, cancellationToken);
        return BuildIndexesByTable(indexes, indexColumns, columnsByTable);
    }
    // islevi: SQL Server PK/unique/FK/check constraint okuma adimlarini turlerine gore ayri helper'larla yurutur.
    protected override async Task<Dictionary<int, List<SchemaConstraintModel>>> GetConstraintsByTableAsync(
        DbContext dbContext,
        List<SchemaDiscoveredTableModel<int>> tables,
        Dictionary<int, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var tableIds = tables.Select(table => table.Id).ToList();
        var constraints = new List<(int TableId, SchemaConstraintModel Constraint)>();
        constraints.AddRange(await ReadKeyConstraintsAsync(
            catalogDbContext,
            tableIds,
            columnsByTable,
            cancellationToken));
        constraints.AddRange(await ReadForeignKeyConstraintsAsync(catalogDbContext, tableIds, cancellationToken));
        constraints.AddRange(await ReadCheckConstraintsAsync(catalogDbContext, tableIds, cancellationToken));
        return GroupConstraintsByTable(constraints);
    }

    // islevi: SQL Server trigger okuma adimini provider helper'ina yonlendirir.
    protected override async Task<Dictionary<int, List<SchemaTriggerModel>>> GetTriggersByTableAsync(
        DbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        return await ReadTriggersByTableAsync(catalogDbContext, tableIds, cancellationToken);
    }

    // islevi: Verilen semadaki tablo/view/procedure/function/trigger nesnelerini tek sys.objects batch sorgusuyla hafif listeler; sema yoksa bos liste doner.
    protected override async Task<List<DatabaseSchemaObjectModel>> ReadObjectsAsync(
        DbContext dbContext,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;

        var schemaId = await catalogDbContext.Set<SqlServerSchemaCatalogRow>()
            .Where(schema => schema.Name == schemaName)
            .Select(schema => (int?)schema.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (schemaId is null)
        {
            return new List<DatabaseSchemaObjectModel>();
        }

        var objects = await catalogDbContext.Set<SqlServerObjectCatalogRow>()
            .Where(dbObject =>
                dbObject.SchemaId == schemaId.Value &&
                !dbObject.IsMsShipped &&
                (dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.UserTableObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.ViewObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.ProcedureObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.ScalarFunctionObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.InlineTableFunctionObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.TableFunctionObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.TriggerObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.SequenceObjectType))
            .Select(dbObject => new { dbObject.Name, dbObject.Type })
            .ToListAsync(cancellationToken);

        var schemaObjects = objects
            .Select(dbObject => new DatabaseSchemaObjectModel
            {
                Schema = schemaName,
                Name = dbObject.Name,
                // sys.objects.type char(2) oldugu icin sondaki bosluk kirpilir; kod eslemesi kirpilmis deger uzerinden yapilir.
                ObjectTypeCode = MapSqlServerObjectType(dbObject.Type.Trim())
            })
            .ToList();
        schemaObjects.AddRange(await ReadTypeObjectsAsync(catalogDbContext, schemaId.Value, schemaName, cancellationToken));
        return schemaObjects;
    }

    // islevi: sys.objects.type kodunu ortak sema nesne tur koduna cevirir; skaler/inline/tablo fonksiyonlarinin tumu Function'a duser.
    private static string MapSqlServerObjectType(string type)
        => type switch
        {
            DatabaseMetadataCatalogConstants.SqlServer.UserTableObjectType => SchemaObjectTypeCodes.Table,
            DatabaseMetadataCatalogConstants.SqlServer.ViewObjectType => SchemaObjectTypeCodes.View,
            DatabaseMetadataCatalogConstants.SqlServer.ProcedureObjectType => SchemaObjectTypeCodes.Procedure,
            DatabaseMetadataCatalogConstants.SqlServer.TriggerObjectType => SchemaObjectTypeCodes.Trigger,
            DatabaseMetadataCatalogConstants.SqlServer.SequenceObjectType => SchemaObjectTypeCodes.Sequence,
            _ => SchemaObjectTypeCodes.Function
        };

    // islevi: sys.types'tan semanin kullanici tanimli tiplerini hafif nesne listesine cevirir.
    private static async Task<List<DatabaseSchemaObjectModel>> ReadTypeObjectsAsync(
        SqlServerCatalogDbContext dbContext,
        int schemaId,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var types = await dbContext.Set<SqlServerTypeCatalogRow>()
            .Where(type => type.SchemaId == schemaId && type.IsUserDefined)
            .Select(type => type.Name)
            .ToListAsync(cancellationToken);

        return types
            .Select(name => new DatabaseSchemaObjectModel
            {
                Schema = schemaName,
                Name = name,
                ObjectTypeCode = SchemaObjectTypeCodes.Type
            })
            .ToList();
    }

    // islevi: View tanimlarini OBJECT_DEFINITION ile snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadViewDefinitionsAsync(
        DbContext dbContext,
        Dictionary<int, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var views = await catalogDbContext.Set<SqlServerObjectCatalogRow>()
            .Where(dbObject =>
                schemaIds.Contains(dbObject.SchemaId) &&
                !dbObject.IsMsShipped &&
                dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.ViewObjectType)
            .Select(dbObject => new
            {
                dbObject.SchemaId,
                dbObject.Name,
                Definition = SqlServerCatalogDbContext.ObjectDefinition(dbObject.Id)
            })
            .ToListAsync(cancellationToken);

        return views
            .Select(view => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[view.SchemaId],
                Name = view.Name,
                ObjectTypeCode = SchemaObjectTypeCodes.View,
                Definition = view.Definition ?? string.Empty
            })
            .ToList();
    }

    // islevi: Procedure/function tanimlarini OBJECT_DEFINITION ile snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadRoutineDefinitionsAsync(
        DbContext dbContext,
        Dictionary<int, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var routines = await catalogDbContext.Set<SqlServerObjectCatalogRow>()
            .Where(dbObject =>
                schemaIds.Contains(dbObject.SchemaId) &&
                !dbObject.IsMsShipped &&
                (dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.ProcedureObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.ScalarFunctionObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.InlineTableFunctionObjectType ||
                 dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.TableFunctionObjectType))
            .Select(dbObject => new
            {
                dbObject.SchemaId,
                dbObject.Name,
                dbObject.Type,
                Definition = SqlServerCatalogDbContext.ObjectDefinition(dbObject.Id)
            })
            .ToListAsync(cancellationToken);

        return routines
            .Select(routine => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[routine.SchemaId],
                Name = routine.Name,
                ObjectTypeCode = routine.Type.Trim() == DatabaseMetadataCatalogConstants.SqlServer.ProcedureObjectType
                    ? SchemaObjectTypeCodes.Procedure
                    : SchemaObjectTypeCodes.Function,
                Definition = routine.Definition ?? string.Empty
            })
            .ToList();
    }

    // islevi: INFORMATION_SCHEMA.SEQUENCES satirlarini snapshot sequence nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadSequenceDefinitionsAsync(
        DbContext dbContext,
        Dictionary<int, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var schemaNames = schemaNameById.Values.ToList();
        var sequences = await catalogDbContext.Set<SqlServerSequenceCatalogRow>()
            .Where(sequence => schemaNames.Contains(sequence.Schema))
            .ToListAsync(cancellationToken);

        return sequences
            .Select(sequence => new SchemaObjectDefinitionModel
            {
                Schema = sequence.Schema,
                Name = sequence.Name,
                ObjectTypeCode = SchemaObjectTypeCodes.Sequence,
                Definition = BuildSequenceDefinition(
                    sequence.DataType,
                    sequence.StartValue,
                    sequence.MinimumValue,
                    sequence.MaximumValue,
                    sequence.Increment,
                    sequence.CycleOption)
            })
            .ToList();
    }

    // islevi: sys.types kullanici tiplerini temel tip bilgileriyle snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadTypeDefinitionsAsync(
        DbContext dbContext,
        Dictionary<int, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (SqlServerCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var types = await catalogDbContext.Set<SqlServerTypeCatalogRow>()
            .Where(type => schemaIds.Contains(type.SchemaId) && type.IsUserDefined)
            .ToListAsync(cancellationToken);
        var systemTypeNames = await ReadSystemTypeNamesByIdAsync(catalogDbContext, types, cancellationToken);

        return types
            .Select(type => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[type.SchemaId],
                Name = type.Name,
                ObjectTypeCode = SchemaObjectTypeCodes.Type,
                Definition = BuildSqlServerTypeDefinition(type, systemTypeNames)
            })
            .ToList();
    }

    // islevi: Kullanici tiplerinin dayandigi sistem tip adlarini tek batch sorguda getirir.
    private static async Task<Dictionary<byte, string>> ReadSystemTypeNamesByIdAsync(
        SqlServerCatalogDbContext dbContext,
        List<SqlServerTypeCatalogRow> userTypes,
        CancellationToken cancellationToken)
    {
        var systemTypeIds = userTypes
            .Select(type => type.SystemTypeId)
            .Distinct()
            .ToList();
        if (systemTypeIds.Count == 0)
        {
            return new Dictionary<byte, string>();
        }

        var systemTypes = await dbContext.Set<SqlServerTypeCatalogRow>()
            .Where(type => systemTypeIds.Contains(type.SystemTypeId) && !type.IsUserDefined)
            .ToListAsync(cancellationToken);

        return systemTypes
            .GroupBy(type => type.SystemTypeId)
            .ToDictionary(group => group.Key, group => group.OrderBy(type => type.Id).First().Name);
    }

    // islevi: SQL Server alias/table type satirini raporlanabilir ortak tanim metnine cevirir.
    private static string BuildSqlServerTypeDefinition(
        SqlServerTypeCatalogRow type,
        Dictionary<byte, string> systemTypeNames)
    {
        if (type.IsTableType)
        {
            return SchemaComparisonTextConstants.DefinitionTokens.TableType;
        }

        var typeName = systemTypeNames.GetValueOrDefault(type.SystemTypeId, type.Name);
        return string.Join(
            SchemaComparisonTextConstants.Normalization.SingleSpace,
            SchemaComparisonTextConstants.DefinitionTokens.AliasAs,
            BuildSqlServerRawDataType(typeName, type.MaxLength, type.Precision, type.Scale));
    }

    // islevi: sys.types uzunluk/precision bilgisini SQL Server ham tip metnine cevirir.
    private static string BuildSqlServerRawDataType(string typeName, short maxLengthBytes, byte precision, byte scale)
    {
        var shape = ResolveSqlServerTypeShape(typeName, maxLengthBytes, precision, scale);
        return BuildRawDataType(typeName, shape.MaxLength, shape.NumericPrecision, shape.NumericScale, shape.IsMaxLength);
    }

    // islevi: Sema filtresini sys.schemas sorgusuna uygular.
    private static IQueryable<SqlServerSchemaCatalogRow> BuildSchemaQuery(
        SqlServerCatalogDbContext dbContext,
        List<string> schemaNames)
    {
        var schemaQuery = dbContext.Set<SqlServerSchemaCatalogRow>();
        return schemaNames.Count > 0
            ? schemaQuery.Where(schema => schemaNames.Contains(schema.Name))
            : schemaQuery.Where(schema =>
                schema.Id < DatabaseMetadataCatalogConstants.SqlServer.SystemSchemaIdLimit &&
                !DatabaseMetadataCatalogConstants.SqlServer.SystemSchemaNames.Contains(schema.Name));
    }

    // islevi: Secilen semalardan kullanici tablo object'lerini filtreleyen sys.objects sorgusunu kurar.
    private static IQueryable<SqlServerObjectCatalogRow> BuildTableQuery(
        SqlServerCatalogDbContext dbContext,
        List<int> schemaIds)
    {
        return dbContext.Set<SqlServerObjectCatalogRow>()
            .Where(dbObject =>
                schemaIds.Contains(dbObject.SchemaId) &&
                !dbObject.IsMsShipped &&
                dbObject.Type == DatabaseMetadataCatalogConstants.SqlServer.UserTableObjectType);
    }

    // islevi: Kolon default ifadelerini tablo/kolon id anahtariyla tek batch okur.
    private static async Task<Dictionary<(int ObjectId, int ColumnId), string>> ReadDefaultSqlByColumnAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<SqlServerDefaultConstraintCatalogRow>()
            .Where(defaultConstraint => tableIds.Contains(defaultConstraint.ParentObjectId))
            .Select(defaultConstraint => new
            {
                defaultConstraint.ParentObjectId,
                defaultConstraint.ParentColumnId,
                defaultConstraint.Definition
            })
            .ToDictionaryAsync(
                defaultConstraint => (defaultConstraint.ParentObjectId, defaultConstraint.ParentColumnId),
                defaultConstraint => defaultConstraint.Definition,
                cancellationToken);
    }

    // islevi: Computed kolon ifade ve persisted alanlarini tablo/kolon anahtariyla tek batch okur.
    private static Task<Dictionary<(int ObjectId, int ColumnId), (string? Definition, bool IsPersisted)>> ReadComputedColumnsByColumnAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<SqlServerComputedColumnCatalogRow>()
            .Where(column => tableIds.Contains(column.ObjectId))
            .ToDictionaryAsync(
                column => (column.ObjectId, column.ColumnId),
                column => ValueTuple.Create(column.Definition, column.IsPersisted),
                cancellationToken);
    }

    // islevi: Identity kolon seed/increment sql_variant degerlerini hassasiyet kaybetmeyen kanonik metin ciftlerine cevirir.
    private static async Task<Dictionary<(int ObjectId, int ColumnId), (string? Seed, string? Increment)>> ReadIdentityValuesByColumnAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var identities = await dbContext.Set<SqlServerIdentityColumnCatalogRow>()
            .Where(column => tableIds.Contains(column.ObjectId))
            .ToListAsync(cancellationToken);

        return identities.ToDictionary(
            column => (column.ObjectId, column.ColumnId),
            column => (ConvertCatalogNumber(column.SeedValue), ConvertCatalogNumber(column.IncrementValue)));
    }

    // islevi: Kolon MS_Description degerlerini tablo/kolon anahtariyla tek batch okur.
    private static async Task<Dictionary<(int ObjectId, int ColumnId), string?>> ReadColumnCommentsByColumnAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var comments = await dbContext.Set<SqlServerExtendedPropertyCatalogRow>()
            .Where(property => property.Class == DatabaseMetadataCatalogConstants.SqlServer.ObjectOrColumnExtendedPropertyClass &&
                               tableIds.Contains(property.MajorId) &&
                               property.MinorId > 0 &&
                               property.Name == DatabaseMetadataCatalogConstants.SqlServer.ColumnDescriptionPropertyName)
            .ToListAsync(cancellationToken);

        return comments
            .GroupBy(comment => (comment.MajorId, comment.MinorId))
            .ToDictionary(group => group.Key, group => Convert.ToString(group.First().Value, CultureInfo.InvariantCulture));
    }

    // islevi: SQL Server sql_variant sayisal katalog degerini invariant ve hassasiyet-kayipsiz metne cevirir.
    private static string? ConvertCatalogNumber(object? value)
        => value is null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);

    // islevi: SQL Server kolonlarini tip bilgisiyle okuyup tablo kimligine gore gruplanmis snapshot kolonlarina cevirir.
    private async Task<Dictionary<int, List<SchemaColumnModel>>> ReadColumnsByTableAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        Dictionary<(int ObjectId, int ColumnId), string> defaultByColumn,
        Dictionary<(int ObjectId, int ColumnId), (string? Definition, bool IsPersisted)> computedByColumn,
        Dictionary<(int ObjectId, int ColumnId), (string? Seed, string? Increment)> identityByColumn,
        Dictionary<(int ObjectId, int ColumnId), string?> commentsByColumn,
        CancellationToken cancellationToken)
    {
        var columns = await ReadColumnRowsAsync(dbContext, tableIds, cancellationToken);
        return columns
            .GroupBy(column => column.Column.ObjectId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(column => column.Column.ColumnId)
                    .Select(column => MapColumn(
                        column.Column,
                        column.TypeName,
                        defaultByColumn.GetValueOrDefault((column.Column.ObjectId, column.Column.ColumnId)),
                        computedByColumn.GetValueOrDefault((column.Column.ObjectId, column.Column.ColumnId)),
                        identityByColumn.GetValueOrDefault((column.Column.ObjectId, column.Column.ColumnId)),
                        commentsByColumn.GetValueOrDefault((column.Column.ObjectId, column.Column.ColumnId))))
                    .ToList());
    }

    // islevi: SQL Server kolon ve type katalog satirlarini tek LINQ sorgusuyla materialize eder.
    private static async Task<List<(SqlServerColumnCatalogRow Column, string TypeName)>> ReadColumnRowsAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.Set<SqlServerColumnCatalogRow>()
            .Where(column => tableIds.Contains(column.ObjectId))
            .Join(
                dbContext.Set<SqlServerTypeCatalogRow>(),
                column => column.UserTypeId,
                type => type.Id,
                (column, type) => new { Column = column, TypeName = type.Name })
            .ToListAsync(cancellationToken);
        return rows.Select(row => (row.Column, row.TypeName)).ToList();
    }

    // islevi: Snapshot kapsamindaki SQL Server index katalog satirlarini okur.
    private static async Task<List<SqlServerIndexCatalogRow>> ReadIndexRowsAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<SqlServerIndexCatalogRow>()
            .Where(index => tableIds.Contains(index.ObjectId) &&
                            index.IndexType > 0 &&
                            index.Name != null)
            .ToListAsync(cancellationToken);
    }
    // islevi: Snapshot kapsamindaki SQL Server index kolon katalog satirlarini okur.
    private static async Task<List<SqlServerIndexColumnCatalogRow>> ReadIndexColumnRowsAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<SqlServerIndexColumnCatalogRow>()
            .Where(indexColumn => tableIds.Contains(indexColumn.ObjectId))
            .ToListAsync(cancellationToken);
    }
    // islevi: Index ve index-kolon katalog satirlarini tablo kimligine gore snapshot index listesine cevirir.
    private static Dictionary<int, List<SchemaIndexModel>> BuildIndexesByTable(
        List<SqlServerIndexCatalogRow> indexes,
        List<SqlServerIndexColumnCatalogRow> indexColumns,
        Dictionary<int, List<SchemaColumnModel>> columnsByTable)
    {
        return indexes
            .GroupBy(index => index.ObjectId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(index => index.Name)
                    .Select(index => MapIndex(
                        index,
                        indexColumns.Where(column => column.ObjectId == index.ObjectId && column.IndexId == index.IndexId).ToList(),
                        BuildColumnNameByOrdinal(columnsByTable.GetValueOrDefault(index.ObjectId))))
                    .ToList());
    }

    // islevi: SQL Server primary key ve unique constraint'lerini index katalogundan snapshot constraint listesine cevirir.
    private static async Task<List<(int TableId, SchemaConstraintModel Constraint)>> ReadKeyConstraintsAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        Dictionary<int, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken)
    {
        var constraintIndexes = await dbContext.Set<SqlServerIndexCatalogRow>()
            .Where(index => tableIds.Contains(index.ObjectId) &&
                            index.IndexType > 0 &&
                            index.Name != null &&
                            (index.IsPrimaryKey || index.IsUniqueConstraint))
            .ToListAsync(cancellationToken);
        var indexColumns = await ReadIndexColumnRowsAsync(dbContext, tableIds, cancellationToken);

        return constraintIndexes
            .Select(index => (
                index.ObjectId,
                MapKeyConstraint(index, indexColumns, columnsByTable)))
            .ToList();
    }

    // islevi: SQL Server PK/unique index satirini ortak constraint modeline cevirir.
    private static SchemaConstraintModel MapKeyConstraint(
        SqlServerIndexCatalogRow index,
        List<SqlServerIndexColumnCatalogRow> indexColumns,
        Dictionary<int, List<SchemaColumnModel>> columnsByTable)
    {
        var columns = MapIndexColumns(
            indexColumns.Where(column => column.ObjectId == index.ObjectId && column.IndexId == index.IndexId).ToList(),
            BuildColumnNameByOrdinal(columnsByTable.GetValueOrDefault(index.ObjectId)),
            includeColumns: false);
        return new SchemaConstraintModel
        {
            Name = NormalizeSystemGeneratedName(index.Name ?? string.Empty, BuildColumnSignature(columns)),
            TypeCode = index.IsPrimaryKey ? SchemaConstraintTypeCodes.PrimaryKey : SchemaConstraintTypeCodes.Unique,
            Columns = columns,
            IsValidated = true,
            IsEnabled = !index.IsDisabled
        };
    }

    // islevi: SQL Server foreign key constraint'lerini sys.foreign_keys uzerinden snapshot constraint listesine cevirir.
    private static async Task<List<(int TableId, SchemaConstraintModel Constraint)>> ReadForeignKeyConstraintsAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var foreignKeys = await dbContext.Set<SqlServerForeignKeyCatalogRow>()
            .Where(foreignKey => tableIds.Contains(foreignKey.ParentObjectId))
            .ToListAsync(cancellationToken);
        var foreignKeyIds = foreignKeys.Select(foreignKey => foreignKey.Id).ToList();
        var foreignKeyColumns = await dbContext.Set<SqlServerForeignKeyColumnCatalogRow>()
            .Where(column => foreignKeyIds.Contains(column.ConstraintObjectId))
            .ToListAsync(cancellationToken);
        var referencedTableIds = foreignKeys.Select(foreignKey => foreignKey.ReferencedObjectId).Distinct().ToList();
        var referencedTables = await GetTableNamesByIdAsync(dbContext, referencedTableIds, cancellationToken);
        var foreignKeyColumnNames = await GetColumnNamesByTableAsync(
            dbContext,
            tableIds.Concat(referencedTableIds).Distinct().ToList(),
            cancellationToken);

        return foreignKeys
            .Select(foreignKey => (
                foreignKey.ParentObjectId,
                MapForeignKeyConstraint(foreignKey, foreignKeyColumns, referencedTables, foreignKeyColumnNames)))
            .ToList();
    }

    // islevi: Tek SQL Server FK satirini ortak snapshot constraint modeline cevirir.
    internal static SchemaConstraintModel MapForeignKeyConstraint(
        SqlServerForeignKeyCatalogRow foreignKey,
        List<SqlServerForeignKeyColumnCatalogRow> foreignKeyColumns,
        Dictionary<int, string> referencedTables,
        Dictionary<(int TableId, int ColumnId), string> foreignKeyColumnNames)
    {
        var keyColumns = foreignKeyColumns
            .Where(column => column.ConstraintObjectId == foreignKey.Id)
            .OrderBy(column => column.ConstraintColumnId)
            .ToList();

        var columns = MapForeignKeyColumnNames(keyColumns, foreignKeyColumnNames, useReferencedColumns: false);
        var referencedTable = referencedTables.GetValueOrDefault(foreignKey.ReferencedObjectId);
        var referencedColumns = MapForeignKeyColumnNames(keyColumns, foreignKeyColumnNames, useReferencedColumns: true);

        return new SchemaConstraintModel
        {
            Name = NormalizeSystemGeneratedName(
                foreignKey.Name,
                BuildColumnSignature(columns.Append(referencedTable ?? string.Empty).Concat(referencedColumns))),
            TypeCode = SchemaConstraintTypeCodes.ForeignKey,
            Columns = columns,
            ReferencedTable = referencedTable,
            ReferencedColumns = referencedColumns,
            DeleteActionCode = MapSqlServerReferentialAction(foreignKey.DeleteReferentialAction),
            UpdateActionCode = MapSqlServerReferentialAction(foreignKey.UpdateReferentialAction),
            IsValidated = !foreignKey.IsNotTrusted,
            IsEnabled = !foreignKey.IsDisabled
        };
    }

    // islevi: SQL Server FK kolon satirlarini kaynak veya hedef kolon adlarina cevirir.
    private static List<string> MapForeignKeyColumnNames(
        List<SqlServerForeignKeyColumnCatalogRow> keyColumns,
        Dictionary<(int TableId, int ColumnId), string> columnNames,
        bool useReferencedColumns)
    {
        return keyColumns
            .Select(column => useReferencedColumns
                ? columnNames.GetValueOrDefault((column.ReferencedObjectId, column.ReferencedColumnId), string.Empty)
                : columnNames.GetValueOrDefault((column.ParentObjectId, column.ParentColumnId), string.Empty))
            .Where(columnName => !string.IsNullOrEmpty(columnName))
            .ToList();
    }
    // islevi: SQL Server check constraint'lerini sys.check_constraints uzerinden snapshot constraint listesine cevirir.
    private static async Task<List<(int TableId, SchemaConstraintModel Constraint)>> ReadCheckConstraintsAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var checkConstraints = await dbContext.Set<SqlServerCheckConstraintCatalogRow>()
            .Where(checkConstraint => tableIds.Contains(checkConstraint.ParentObjectId) && !checkConstraint.IsMsShipped)
            .ToListAsync(cancellationToken);

        return checkConstraints
            .Select(checkConstraint => (
                checkConstraint.ParentObjectId,
                new SchemaConstraintModel
                {
                    Name = NormalizeSystemGeneratedName(checkConstraint.Name, checkConstraint.Definition ?? string.Empty),
                    TypeCode = SchemaConstraintTypeCodes.Check,
                    Definition = checkConstraint.Definition,
                    IsValidated = !checkConstraint.IsNotTrusted,
                    IsEnabled = !checkConstraint.IsDisabled
                }))
            .ToList();
    }
    // islevi: Farkli constraint turlerinden gelen tablo/constraint ciftlerini tablo kimligine gore gruplayip siralar.
    private static Dictionary<int, List<SchemaConstraintModel>> GroupConstraintsByTable(
        List<(int TableId, SchemaConstraintModel Constraint)> constraints)
    {
        return constraints
            .GroupBy(item => item.TableId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Constraint)
                    .OrderBy(constraint => constraint.TypeCode)
                    .ThenBy(constraint => constraint.Name)
                    .ToList());
    }

    // islevi: SQL Server trigger katalog satirlarini okuyup tablo kimligine gore snapshot trigger listesine cevirir.
    private static async Task<Dictionary<int, List<SchemaTriggerModel>>> ReadTriggersByTableAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var triggers = await dbContext.Set<SqlServerTriggerCatalogRow>()
            .Where(trigger => tableIds.Contains(trigger.ParentObjectId) && !trigger.IsMsShipped)
            .Select(trigger => new
            {
                trigger.ParentObjectId,
                trigger.Name,
                trigger.IsDisabled,
                Definition = SqlServerCatalogDbContext.ObjectDefinition(trigger.Id)
            })
            .ToListAsync(cancellationToken);

        return triggers
            .GroupBy(trigger => trigger.ParentObjectId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(trigger => trigger.Name)
                    .Select(trigger => new SchemaTriggerModel
                    {
                        Name = trigger.Name,
                        Definition = trigger.Definition ?? string.Empty,
                        IsEnabled = !trigger.IsDisabled
                    })
                    .ToList());
    }
    // islevi: sys.columns + sys.types ham alanlarindan kanonik kolon modeli uretir; max_length'in bayt/MAX ve precision/scale cozumu burada.
    private SchemaColumnModel MapColumn(
        SqlServerColumnCatalogRow column,
        string typeName,
        string? defaultValueSql,
        (string? Definition, bool IsPersisted) computed,
        (string? Seed, string? Increment) identity,
        string? comment)
    {
        var shape = ResolveSqlServerTypeShape(typeName, column.MaxLength, column.Precision, column.Scale);
        var model = CreateColumnModel(column, typeName, shape);
        MapColumnDepth(model, column, defaultValueSql, computed, identity, comment);
        return ApplyTypeMapping(model, typeName);
    }

    // islevi: SQL Server kolonunun ad/tip/null/boyut/identity temel alanlarini ortak modele kurar.
    private static SchemaColumnModel CreateColumnModel(
        SqlServerColumnCatalogRow column,
        string typeName,
        DatabaseColumnTypeShapeModel shape)
        => new()
        {
            Name = column.Name,
            Ordinal = column.ColumnId,
            RawDataType = BuildRawDataType(typeName, shape.MaxLength, shape.NumericPrecision, shape.NumericScale, shape.IsMaxLength),
            IsNullable = column.IsNullable,
            MaxLength = shape.MaxLength,
            NumericPrecision = shape.NumericPrecision,
            NumericScale = shape.NumericScale,
            IsIdentity = column.IsIdentity
        };

    // islevi: SQL Server kolonunun computed/collation/identity-sequence/comment alanlarini ortak modele ekler.
    private static void MapColumnDepth(
        SchemaColumnModel model,
        SqlServerColumnCatalogRow column,
        string? defaultValueSql,
        (string? Definition, bool IsPersisted) computed,
        (string? Seed, string? Increment) identity,
        string? comment)
    {
        model.DefaultValueSql = defaultValueSql;
        model.CollationName = column.CollationName;
        model.IsGenerated = column.IsComputed;
        model.GenerationExpression = computed.Definition;
        model.IsPersisted = computed.IsPersisted;
        model.IdentitySeed = identity.Seed;
        model.IdentityIncrement = identity.Increment;
        model.Comment = comment;
    }

    // islevi: SQL Server max_length/precision/scale alanlarini ortak boyut sekline tek merkezde cozer.
    private static DatabaseColumnTypeShapeModel ResolveSqlServerTypeShape(
        string typeName,
        short maxLengthBytes,
        byte precision,
        byte scale)
        => typeName switch
        {
            DatabaseMetadataCatalogConstants.SqlServer.VarCharTypeName or
                DatabaseMetadataCatalogConstants.SqlServer.CharTypeName => ResolveSqlServerTextShape(maxLengthBytes),
            DatabaseMetadataCatalogConstants.SqlServer.NVarCharTypeName or
                DatabaseMetadataCatalogConstants.SqlServer.NCharTypeName => ResolveSqlServerUnicodeTextShape(maxLengthBytes),
            DatabaseMetadataCatalogConstants.SqlServer.VarBinaryTypeName or
                DatabaseMetadataCatalogConstants.SqlServer.BinaryTypeName => ResolveSqlServerBinaryShape(maxLengthBytes),
            DatabaseMetadataCatalogConstants.SqlServer.DecimalTypeName or
                DatabaseMetadataCatalogConstants.SqlServer.NumericTypeName => ResolveDecimalShape(precision, scale),
            _ => new DatabaseColumnTypeShapeModel()
        };

    // islevi: Tek-baytli SQL Server metin tipinin karakter uzunlugunu cozer.
    private static DatabaseColumnTypeShapeModel ResolveSqlServerTextShape(short maxLengthBytes)
        => ResolveSqlServerLengthShape(maxLengthBytes, 1);

    // islevi: Unicode SQL Server metin tipinin bayt uzunlugunu karakter uzunluguna cevirir.
    private static DatabaseColumnTypeShapeModel ResolveSqlServerUnicodeTextShape(short maxLengthBytes)
        => ResolveSqlServerLengthShape(
            maxLengthBytes,
            DatabaseMetadataCatalogConstants.SqlServer.UnicodeBytesPerChar);

    // islevi: SQL Server ikili tipinin azami bayt uzunlugunu cozer.
    private static DatabaseColumnTypeShapeModel ResolveSqlServerBinaryShape(short maxLengthBytes)
        => ResolveSqlServerLengthShape(maxLengthBytes, 1);

    // islevi: SQL Server uzunluk alanini normal veya MAX sekline indirger.
    private static DatabaseColumnTypeShapeModel ResolveSqlServerLengthShape(short maxLengthBytes, int divisor)
        => maxLengthBytes == DatabaseMetadataCatalogConstants.SqlServer.MaxLengthSentinel
            ? new DatabaseColumnTypeShapeModel { IsMaxLength = true }
            : new DatabaseColumnTypeShapeModel { MaxLength = maxLengthBytes / divisor };

    // islevi: SQL Server ondalik tipinin precision ve scale alanlarini kanonik sekle tasir.
    private static DatabaseColumnTypeShapeModel ResolveDecimalShape(byte precision, byte scale)
        => new() { NumericPrecision = precision, NumericScale = scale };
    // islevi: SQL Server index katalog satirini kanonik index modeline cevirir.
    private static SchemaIndexModel MapIndex(
        SqlServerIndexCatalogRow index,
        List<SqlServerIndexColumnCatalogRow> indexColumns,
        Dictionary<int, string> columnNameByOrdinal)
    {
        var keyColumns = MapIndexColumns(indexColumns, columnNameByOrdinal, includeColumns: false);

        return new SchemaIndexModel
        {
            Name = NormalizeSystemGeneratedName(index.Name ?? string.Empty, BuildColumnSignature(keyColumns)),
            IsUnique = index.IsUnique,
            IsPrimaryKey = index.IsPrimaryKey,
            Columns = keyColumns,
            IncludedColumns = MapIndexColumns(indexColumns, columnNameByOrdinal, includeColumns: true),
            FilterDefinition = index.FilterDefinition,
            Definition = null
        };
    }

    // islevi: SQL Server'in ozel ad verilmemis kisita/index'e atadigi rastgele-hex sistem adini, iki veritabaninda da ayni cikacak deterministik (tip + yapisal imza) kanonik ada cevirir; kullanicinin verdigi adlara dokunmaz.
    // sistemdeki gorevi: Diff motoru bu objeleri ADLA eslestirir; SQL Server auto-name'deki rastgele hex her veritabaninda farkli oldugu icin ayni kisit "biri silindi + biri eklendi" yalanci farki (false positive) uretirdi. Kanonik adi okuma aninda (kaynakta) uretip bu yalanci farki keser. PostgreSQL auto-name'leri deterministik oldugu icin normalizasyon yalniz SQL Server okuyucusunda gerekir; yapisal imza bos ise ad korunur ki ayirt edici olmadan farkli objeler yanlislikla birlestirilmesin.
    private static string NormalizeSystemGeneratedName(string name, string structuralSignature)
    {
        if (string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(structuralSignature) ||
            !SqlServerSystemNamePattern().IsMatch(name))
        {
            return name;
        }

        var separator = ComparisonCanonicalTextConstants.SystemGeneratedNameSeparator;
        var typePrefix = name.Substring(0, name.IndexOf(separator, StringComparison.Ordinal));
        return $"{typePrefix}{separator}{structuralSignature}";
    }

    // islevi: Kolon/imza parcalarini deterministik tek bir yapisal imza string'ine cevirir (sistem-adi normalizasyonunun ayirt edicisi).
    private static string BuildColumnSignature(IEnumerable<string> signatureParts)
        => string.Join(ComparisonCanonicalTextConstants.SignaturePartSeparator, signatureParts);
    // islevi: Index kolon satirlarini anahtar/include ayrimina gore kolon adlarina cevirir.
    private static List<string> MapIndexColumns(
        List<SqlServerIndexColumnCatalogRow> indexColumns,
        Dictionary<int, string> columnNameByOrdinal,
        bool includeColumns)
    {
        return indexColumns
            .Where(column => includeColumns ? column.IsIncludedColumn : !column.IsIncludedColumn && column.KeyOrdinal > 0)
            .OrderBy(column => includeColumns ? column.ColumnId : column.KeyOrdinal)
            .Select(column => columnNameByOrdinal.GetValueOrDefault(column.ColumnId, string.Empty))
            .Where(columnName => !string.IsNullOrEmpty(columnName))
            .ToList();
    }
    // islevi: SchemaColumnModel listesinden kolon numarasi -> kolon adi sozlugu kurar.
    private static Dictionary<int, string> BuildColumnNameByOrdinal(List<SchemaColumnModel>? columns)
        => columns?.ToDictionary(column => column.Ordinal, column => column.Name) ?? new Dictionary<int, string>();
    // islevi: SQL Server object_id listesini schema.table adlarina cevirir.
    private static async Task<Dictionary<int, string>> GetTableNamesByIdAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<SqlServerObjectCatalogRow>()
            .Where(dbObject => tableIds.Contains(dbObject.Id))
            .Join(
                dbContext.Set<SqlServerSchemaCatalogRow>(),
                dbObject => dbObject.SchemaId,
                schema => schema.Id,
                (dbObject, schema) => new
                {
                    dbObject.Id,
                    FullName = schema.Name + "." + dbObject.Name
                })
            .ToDictionaryAsync(table => table.Id, table => table.FullName, cancellationToken);
    }
    // islevi: Tablo/kolon id ciftlerini kolon adlarina ceviren sozlugu tek batch sorguyla kurar.
    private static async Task<Dictionary<(int TableId, int ColumnId), string>> GetColumnNamesByTableAsync(
        SqlServerCatalogDbContext dbContext,
        List<int> tableIds,
        CancellationToken cancellationToken)
    {
        var columns = await dbContext.Set<SqlServerColumnCatalogRow>()
            .Where(column => tableIds.Contains(column.ObjectId))
            .Select(column => new { column.ObjectId, column.ColumnId, column.Name })
            .ToListAsync(cancellationToken);

        return columns.ToDictionary(column => (column.ObjectId, column.ColumnId), column => column.Name);
    }
    // islevi: SQL Server sayisal FK davranis kodunu ortak snapshot koduna cevirir.
    private static string MapSqlServerReferentialAction(byte action)
        => action switch
        {
            DatabaseMetadataCatalogConstants.SqlServer.NoActionReferentialAction => SchemaReferentialActionCodes.NoAction,
            DatabaseMetadataCatalogConstants.SqlServer.CascadeReferentialAction => SchemaReferentialActionCodes.Cascade,
            DatabaseMetadataCatalogConstants.SqlServer.SetNullReferentialAction => SchemaReferentialActionCodes.SetNull,
            DatabaseMetadataCatalogConstants.SqlServer.SetDefaultReferentialAction => SchemaReferentialActionCodes.SetDefault,
            _ => SchemaReferentialActionCodes.Unknown
        };
}
