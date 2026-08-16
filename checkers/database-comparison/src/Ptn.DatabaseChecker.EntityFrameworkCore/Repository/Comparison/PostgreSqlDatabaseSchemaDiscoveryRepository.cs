using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.PostgreSql;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: PostgreSQL pg_catalog metadata'sini EF Core method-syntax LINQ ile okuyup motor-bagimsiz SchemaSnapshotModel'e cevirir.
// sistemdeki gorevi: Bir baglantinin verilen semalarinin tam fotografini (tablo, kolon, default, index, constraint, trigger) uretir; provider'a ozel pg_catalog okumalari base snapshot akisina hook olarak baglanir.
public class PostgreSqlDatabaseSchemaDiscoveryRepository
    : DatabaseSchemaDiscoveryRepositoryBase<uint>, ITransientDependency
{
    public PostgreSqlDatabaseSchemaDiscoveryRepository()
    {
    }

    // islevi: PostgreSQL discovery akisina engine type-map resolver bagimliligini aktarir.
    public PostgreSqlDatabaseSchemaDiscoveryRepository(
        IEngineComponentResolver<IEngineTypeMapProvider> typeMapProviderResolver)
        : base(typeMapProviderResolver)
    {
    }

    public override string EngineCode => DatabaseEngineCodes.PostgreSql;

    // islevi: Izinli tek PostgreSQL setting adinin etkin degerini pg_settings CatalogRow uzerinden LINQ ile okur.
    public async Task<string?> ReadSettingAsync(
        DatabaseConnectionInfo info,
        string settingName,
        CancellationToken cancellationToken = default)
    {
        await using var catalogContext = PostgreSqlCatalogDbContext.Create(info);
        return await catalogContext.Set<PostgreSqlSettingCatalogRow>()
            .Where(setting => setting.Name == settingName)
            .Select(setting => setting.Setting)
            .SingleOrDefaultAsync(cancellationToken);
    }

    // islevi: PostgreSQL runtime katalog context'ini base snapshot akisina verir.
    protected override DbContext CreateDbContext(DatabaseConnectionInfo info)
        => PostgreSqlCatalogDbContext.Create(info);

    // islevi: PostgreSQL veritabani collation adini ve varsa provider kodunu snapshot basligina tasir.
    protected override async Task PopulateSnapshotMetadataAsync(
        DbContext dbContext,
        SchemaSnapshotModel snapshot,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        snapshot.DatabaseCollationName = await catalogDbContext.Set<PostgreSqlDatabaseCatalogRow>()
            .Where(database => database.Name == snapshot.DatabaseName)
            .Select(database => database.CollationName)
            .FirstOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(snapshot.DatabaseCollationName))
        {
            return;
        }

        snapshot.CollationProviderCode = await ReadCollationProviderCodeAsync(
            catalogDbContext,
            snapshot.DatabaseCollationName,
            cancellationToken);
    }

    // islevi: Okunacak PostgreSQL semalarini oid -> ad sozlugu olarak dondurur.
    protected override Task<Dictionary<uint, string>> GetSchemaNameByIdAsync(
        DbContext dbContext,
        List<string> schemaNames,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        return BuildSchemaQuery(catalogDbContext, schemaNames)
            .ToDictionaryAsync(schema => schema.Id, schema => schema.Name, cancellationToken);
    }

    // islevi: Secilen semalardaki PostgreSQL tablo ve partitioned-table relation'larini dondurur.
    protected override Task<List<SchemaDiscoveredTableModel<uint>>> GetTablesAsync(
        DbContext dbContext,
        List<uint> schemaIds,
        CancellationToken cancellationToken)
        => GetTablesAsync(dbContext, schemaIds, null, cancellationToken);

    protected override Task<List<SchemaDiscoveredTableModel<uint>>> GetTablesAsync(
        DbContext dbContext,
        List<uint> schemaIds,
        string? tableName,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var query = BuildTableQuery(catalogDbContext, schemaIds);
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            query = query.Where(relation => relation.Name == tableName);
        }

        return query
            .Select(relation => new SchemaDiscoveredTableModel<uint>
            {
                Id = relation.Id,
                SchemaId = relation.NamespaceId,
                Name = relation.Name
            })
            .ToListAsync(cancellationToken);
    }

    // islevi: PostgreSQL tablo kolonlarini pg_attribute + pg_type join'iyle okur ve tablo kimligine gore gruplar.
    protected override async Task<Dictionary<uint, List<SchemaColumnModel>>> GetColumnsByTableAsync(
        DbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var expressions = await ReadColumnExpressionsByColumnAsync(catalogDbContext, tableIds, cancellationToken);
        var identities = await ReadIdentityValuesByColumnAsync(catalogDbContext, tableIds, cancellationToken);
        var comments = await ReadColumnCommentsByColumnAsync(catalogDbContext, tableIds, cancellationToken);
        return await ReadColumnsByTableAsync(
            catalogDbContext,
            tableIds,
            expressions,
            identities,
            comments,
            cancellationToken);
    }

    // islevi: PostgreSQL index okuma adimini provider helper'ina yonlendirir.
    protected override async Task<Dictionary<uint, List<SchemaIndexModel>>> GetIndexesByTableAsync(
        DbContext dbContext,
        List<uint> tableIds,
        Dictionary<uint, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        return await ReadIndexesByTableAsync(catalogDbContext, tableIds, columnsByTable, cancellationToken);
    }

    // islevi: PostgreSQL PK/unique/FK/check constraint okuma adimlarini ayri helper'larla yurutur.
    protected override async Task<Dictionary<uint, List<SchemaConstraintModel>>> GetConstraintsByTableAsync(
        DbContext dbContext,
        List<SchemaDiscoveredTableModel<uint>> tables,
        Dictionary<uint, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken)
    {
        _ = columnsByTable;
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var tableIds = tables.Select(table => table.Id).ToList();
        var constraints = await ReadSupportedConstraintRowsAsync(catalogDbContext, tableIds, cancellationToken);
        var definitions = await ReadConstraintDefinitionsByIdAsync(catalogDbContext, constraints, cancellationToken);
        var foreignTableIds = GetForeignTableIds(constraints);
        var referencedTableNames = await GetTableNamesByIdAsync(catalogDbContext, foreignTableIds, cancellationToken);
        var columnNames = await GetColumnNamesByTableAsync(
            catalogDbContext,
            tableIds.Concat(foreignTableIds).Distinct().ToList(),
            cancellationToken);

        return BuildConstraintsByTable(constraints, definitions, referencedTableNames, columnNames);
    }

    // islevi: PostgreSQL trigger okuma adimini provider helper'ina yonlendirir.
    protected override async Task<Dictionary<uint, List<SchemaTriggerModel>>> GetTriggersByTableAsync(
        DbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        return await ReadTriggersByTableAsync(catalogDbContext, tableIds, cancellationToken);
    }

    // islevi: Verilen semadaki tablo/view/function/procedure/trigger nesnelerini hafif listeler; her tur ayri batch sorgudur, sema yoksa bos liste doner.
    protected override async Task<List<DatabaseSchemaObjectModel>> ReadObjectsAsync(
        DbContext dbContext,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;

        var schemaId = await catalogDbContext.Set<PostgreSqlNamespaceCatalogRow>()
            .Where(schema => schema.Name == schemaName)
            .Select(schema => (uint?)schema.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (schemaId is null)
        {
            return new List<DatabaseSchemaObjectModel>();
        }

        var objects = new List<DatabaseSchemaObjectModel>();
        objects.AddRange(await ReadRelationObjectsAsync(catalogDbContext, schemaId.Value, schemaName, cancellationToken));
        objects.AddRange(await ReadRoutineObjectsAsync(catalogDbContext, schemaId.Value, schemaName, cancellationToken));
        objects.AddRange(await ReadTriggerObjectsAsync(catalogDbContext, schemaId.Value, schemaName, cancellationToken));
        objects.AddRange(await ReadTypeObjectsAsync(catalogDbContext, schemaId.Value, schemaName, cancellationToken));
        objects.AddRange(await ReadExtensionObjectsAsync(catalogDbContext, schemaId.Value, schemaName, cancellationToken));
        return objects;
    }

    // islevi: pg_class'tan semanin tablo/partitioned-table/view/materialized-view relation'larini tek batch okur.
    private static async Task<List<DatabaseSchemaObjectModel>> ReadRelationObjectsAsync(
        PostgreSqlCatalogDbContext dbContext,
        uint schemaId,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var relations = await dbContext.Set<PostgreSqlClassCatalogRow>()
            .Where(relation =>
                relation.NamespaceId == schemaId &&
                (relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.TableRelKind ||
                 relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.PartitionedTableRelKind ||
                 relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.ViewRelKind ||
                 relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.MaterializedViewRelKind ||
                 relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.SequenceRelKind))
            .Select(relation => new { relation.Name, relation.Kind })
            .ToListAsync(cancellationToken);

        return relations
            .Select(relation => new DatabaseSchemaObjectModel
            {
                Schema = schemaName,
                Name = relation.Name,
                ObjectTypeCode = MapRelationObjectType(relation.Kind)
            })
            .ToList();
    }

    // islevi: pg_proc'tan semanin function/procedure rutinlerini tek batch okur.
    private static async Task<List<DatabaseSchemaObjectModel>> ReadRoutineObjectsAsync(
        PostgreSqlCatalogDbContext dbContext,
        uint schemaId,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var routines = await dbContext.Set<PostgreSqlProcedureCatalogRow>()
            .Where(routine =>
                routine.NamespaceId == schemaId &&
                (routine.Kind == DatabaseMetadataCatalogConstants.PostgreSql.FunctionProKind ||
                 routine.Kind == DatabaseMetadataCatalogConstants.PostgreSql.ProcedureProKind))
            .Select(routine => new { routine.Name, routine.Kind })
            .ToListAsync(cancellationToken);

        return routines
            .Select(routine => new DatabaseSchemaObjectModel
            {
                Schema = schemaName,
                Name = routine.Name,
                ObjectTypeCode = routine.Kind == DatabaseMetadataCatalogConstants.PostgreSql.ProcedureProKind
                    ? SchemaObjectTypeCodes.Procedure
                    : SchemaObjectTypeCodes.Function
            })
            .ToList();
    }

    // islevi: pg_trigger'i pg_class'a join'leyip semaya bagli tablolarin kullanici trigger'larini tek batch okur.
    private static async Task<List<DatabaseSchemaObjectModel>> ReadTriggerObjectsAsync(
        PostgreSqlCatalogDbContext dbContext,
        uint schemaId,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var triggerNames = await dbContext.Set<PostgreSqlTriggerCatalogRow>()
            .Where(trigger => !trigger.IsInternal)
            .Join(
                dbContext.Set<PostgreSqlClassCatalogRow>().Where(relation => relation.NamespaceId == schemaId),
                trigger => trigger.RelationId,
                relation => relation.Id,
                (trigger, relation) => trigger.Name)
            .ToListAsync(cancellationToken);

        return triggerNames
            .Select(name => new DatabaseSchemaObjectModel
            {
                Schema = schemaName,
                Name = name,
                ObjectTypeCode = SchemaObjectTypeCodes.Trigger
            })
            .ToList();
    }

    // islevi: pg_type'tan semanin enum/domain tiplerini hafif nesne listesine cevirir.
    private static async Task<List<DatabaseSchemaObjectModel>> ReadTypeObjectsAsync(
        PostgreSqlCatalogDbContext dbContext,
        uint schemaId,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var types = await dbContext.Set<PostgreSqlTypeCatalogRow>()
            .Where(type =>
                type.NamespaceId == schemaId &&
                (type.TypeKind == DatabaseMetadataCatalogConstants.PostgreSql.EnumTypeKind ||
                 type.TypeKind == DatabaseMetadataCatalogConstants.PostgreSql.DomainTypeKind))
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

    // islevi: pg_extension'dan semaya kurulu extension'lari hafif nesne listesine cevirir.
    private static async Task<List<DatabaseSchemaObjectModel>> ReadExtensionObjectsAsync(
        PostgreSqlCatalogDbContext dbContext,
        uint schemaId,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var extensions = await dbContext.Set<PostgreSqlExtensionCatalogRow>()
            .Where(extension => extension.NamespaceId == schemaId)
            .Select(extension => extension.Name)
            .ToListAsync(cancellationToken);

        return extensions
            .Select(name => new DatabaseSchemaObjectModel
            {
                Schema = schemaName,
                Name = name,
                ObjectTypeCode = SchemaObjectTypeCodes.Extension
            })
            .ToList();
    }

    // islevi: pg_class relkind kodunu ortak sema nesne tur koduna cevirir (view ailesi View, digerleri Table).
    private static string MapRelationObjectType(string relKind)
        => relKind switch
        {
            DatabaseMetadataCatalogConstants.PostgreSql.ViewRelKind => SchemaObjectTypeCodes.View,
            DatabaseMetadataCatalogConstants.PostgreSql.MaterializedViewRelKind => SchemaObjectTypeCodes.View,
            DatabaseMetadataCatalogConstants.PostgreSql.SequenceRelKind => SchemaObjectTypeCodes.Sequence,
            _ => SchemaObjectTypeCodes.Table
        };

    // islevi: View/materialized view tanimlarini schema-level snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadViewDefinitionsAsync(
        DbContext dbContext,
        Dictionary<uint, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var views = await catalogDbContext.Set<PostgreSqlClassCatalogRow>()
            .Where(relation =>
                schemaIds.Contains(relation.NamespaceId) &&
                (relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.ViewRelKind ||
                 relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.MaterializedViewRelKind))
            .Select(relation => new
            {
                relation.NamespaceId,
                relation.Name,
                relation.Kind,
                Definition = PostgreSqlCatalogDbContext.GetViewDefinition(relation.Id, true)
            })
            .ToListAsync(cancellationToken);

        return views
            .Select(view => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[view.NamespaceId],
                Name = view.Name,
                ObjectTypeCode = SchemaObjectTypeCodes.View,
                Definition = BuildPostgreSqlViewDefinition(view.Kind, view.Definition)
            })
            .ToList();
    }

    // islevi: Function/procedure tanimlarini imzali adlariyla snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadRoutineDefinitionsAsync(
        DbContext dbContext,
        Dictionary<uint, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var routines = await catalogDbContext.Set<PostgreSqlProcedureCatalogRow>()
            .Where(routine =>
                schemaIds.Contains(routine.NamespaceId) &&
                (routine.Kind == DatabaseMetadataCatalogConstants.PostgreSql.FunctionProKind ||
                 routine.Kind == DatabaseMetadataCatalogConstants.PostgreSql.ProcedureProKind))
            .Select(routine => new
            {
                routine.NamespaceId,
                routine.Name,
                routine.Kind,
                Arguments = PostgreSqlCatalogDbContext.GetFunctionIdentityArguments(routine.Id),
                Definition = PostgreSqlCatalogDbContext.GetFunctionDefinition(routine.Id)
            })
            .ToListAsync(cancellationToken);

        return routines
            .Select(routine => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[routine.NamespaceId],
                Name = BuildRoutineName(routine.Name, routine.Arguments),
                ObjectTypeCode = routine.Kind == DatabaseMetadataCatalogConstants.PostgreSql.ProcedureProKind
                    ? SchemaObjectTypeCodes.Procedure
                    : SchemaObjectTypeCodes.Function,
                Definition = routine.Definition ?? string.Empty
            })
            .ToList();
    }

    // islevi: Sequence ayarlarini pg_class + pg_sequence join'iyle snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadSequenceDefinitionsAsync(
        DbContext dbContext,
        Dictionary<uint, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var sequences = await catalogDbContext.Set<PostgreSqlClassCatalogRow>()
            .Where(relation =>
                schemaIds.Contains(relation.NamespaceId) &&
                relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.SequenceRelKind)
            .Join(
                catalogDbContext.Set<PostgreSqlSequenceCatalogRow>(),
                relation => relation.Id,
                sequence => sequence.SequenceRelId,
                (relation, sequence) => new
                {
                    relation.NamespaceId,
                    relation.Name,
                    sequence.StartValue,
                    sequence.MinimumValue,
                    sequence.MaximumValue,
                    sequence.Increment,
                    sequence.CacheValue,
                    sequence.IsCycling
                })
            .ToListAsync(cancellationToken);

        return sequences
            .Select(sequence => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[sequence.NamespaceId],
                Name = sequence.Name,
                ObjectTypeCode = SchemaObjectTypeCodes.Sequence,
                Definition = BuildSequenceDefinition(
                    SchemaComparisonTextConstants.DefinitionTokens.BigInt,
                    sequence.StartValue.ToString(),
                    sequence.MinimumValue.ToString(),
                    sequence.MaximumValue.ToString(),
                    sequence.Increment.ToString(),
                    sequence.IsCycling
                        ? SchemaComparisonTextConstants.DefinitionTokens.Yes
                        : SchemaComparisonTextConstants.DefinitionTokens.No,
                    sequence.CacheValue.ToString())
            })
            .ToList();
    }

    // islevi: Enum/domain tiplerini etiketleri veya temel tipleriyle snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadTypeDefinitionsAsync(
        DbContext dbContext,
        Dictionary<uint, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var types = await catalogDbContext.Set<PostgreSqlTypeCatalogRow>()
            .Where(type =>
                schemaIds.Contains(type.NamespaceId) &&
                (type.TypeKind == DatabaseMetadataCatalogConstants.PostgreSql.EnumTypeKind ||
                 type.TypeKind == DatabaseMetadataCatalogConstants.PostgreSql.DomainTypeKind))
            .ToListAsync(cancellationToken);
        var enumLabels = await ReadEnumLabelsByTypeAsync(catalogDbContext, types, cancellationToken);
        var baseTypeNames = await ReadBaseTypeNamesByIdAsync(catalogDbContext, types, cancellationToken);

        return types
            .Select(type => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[type.NamespaceId],
                Name = type.Name,
                ObjectTypeCode = SchemaObjectTypeCodes.Type,
                Definition = BuildPostgreSqlTypeDefinition(type, enumLabels, baseTypeNames)
            })
            .ToList();
    }

    // islevi: Extension ad ve versiyonlarini snapshot nesnelerine cevirir.
    protected override async Task<List<SchemaObjectDefinitionModel>> ReadExtensionDefinitionsAsync(
        DbContext dbContext,
        Dictionary<uint, string> schemaNameById,
        CancellationToken cancellationToken)
    {
        var catalogDbContext = (PostgreSqlCatalogDbContext)dbContext;
        var schemaIds = schemaNameById.Keys.ToList();
        var extensions = await catalogDbContext.Set<PostgreSqlExtensionCatalogRow>()
            .Where(extension => schemaIds.Contains(extension.NamespaceId))
            .ToListAsync(cancellationToken);

        return extensions
            .Select(extension => new SchemaObjectDefinitionModel
            {
                Schema = schemaNameById[extension.NamespaceId],
                Name = extension.Name,
                ObjectTypeCode = SchemaObjectTypeCodes.Extension,
                Definition = string.Join(
                    SchemaComparisonTextConstants.Normalization.SingleSpace,
                    SchemaComparisonTextConstants.DefinitionTokens.ExtensionVersion,
                    extension.Version)
            })
            .ToList();
    }

    // islevi: View ve materialized view ayrimini tanim metnine ekleyerek ayni SELECT metnindeki tur farkini kaybettirmez.
    private static string BuildPostgreSqlViewDefinition(string relationKind, string? definition)
    {
        var viewKind = relationKind == DatabaseMetadataCatalogConstants.PostgreSql.MaterializedViewRelKind
            ? SchemaComparisonTextConstants.DefinitionTokens.MaterializedView
            : SchemaComparisonTextConstants.DefinitionTokens.View;
        return string.Join(
            SchemaComparisonTextConstants.Normalization.SingleSpace,
            viewKind,
            definition ?? string.Empty);
    }

    // islevi: PostgreSQL overloaded routine'lerde adi arguman imzasiyla birlikte kararli hale getirir.
    private static string BuildRoutineName(string name, string? arguments)
        => $"{name}({arguments ?? string.Empty})";

    // islevi: Snapshot kapsamindaki enum tip etiketlerini type id -> sirali label listesi olarak okur.
    private static async Task<Dictionary<uint, List<string>>> ReadEnumLabelsByTypeAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<PostgreSqlTypeCatalogRow> types,
        CancellationToken cancellationToken)
    {
        var enumTypeIds = types
            .Where(type => type.TypeKind == DatabaseMetadataCatalogConstants.PostgreSql.EnumTypeKind)
            .Select(type => type.Id)
            .ToList();
        if (enumTypeIds.Count == 0)
        {
            return new Dictionary<uint, List<string>>();
        }

        var labels = await dbContext.Set<PostgreSqlEnumCatalogRow>()
            .Where(label => enumTypeIds.Contains(label.TypeId))
            .ToListAsync(cancellationToken);

        return labels
            .GroupBy(label => label.TypeId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(label => label.SortOrder)
                    .Select(label => label.Name)
                    .ToList());
    }

    // islevi: Domain tipleri icin temel tip adlarini tek batch sorguda getirir.
    private static async Task<Dictionary<uint, string>> ReadBaseTypeNamesByIdAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<PostgreSqlTypeCatalogRow> types,
        CancellationToken cancellationToken)
    {
        var baseTypeIds = types
            .Where(type => type.BaseTypeId != 0)
            .Select(type => type.BaseTypeId)
            .Distinct()
            .ToList();
        if (baseTypeIds.Count == 0)
        {
            return new Dictionary<uint, string>();
        }

        return await dbContext.Set<PostgreSqlTypeCatalogRow>()
            .Where(type => baseTypeIds.Contains(type.Id))
            .ToDictionaryAsync(type => type.Id, type => type.Name, cancellationToken);
    }

    // islevi: PostgreSQL enum/domain tipini raporlanabilir ortak tanim metnine cevirir.
    private static string BuildPostgreSqlTypeDefinition(
        PostgreSqlTypeCatalogRow type,
        Dictionary<uint, List<string>> enumLabels,
        Dictionary<uint, string> baseTypeNames)
    {
        if (type.TypeKind == DatabaseMetadataCatalogConstants.PostgreSql.EnumTypeKind)
        {
            var labels = enumLabels.GetValueOrDefault(type.Id) ?? new List<string>();
            return $"{SchemaComparisonTextConstants.DefinitionTokens.Enum} ({string.Join(", ", labels)})";
        }

        var baseTypeName = baseTypeNames.GetValueOrDefault(type.BaseTypeId, string.Empty);
        return string.Join(
            SchemaComparisonTextConstants.Normalization.SingleSpace,
            SchemaComparisonTextConstants.DefinitionTokens.DomainAs,
            baseTypeName);
    }

    // islevi: Veritabani collation adiyla eslesen ilk PostgreSQL provider kodunu dondurur.
    private static Task<string?> ReadCollationProviderCodeAsync(
        PostgreSqlCatalogDbContext dbContext,
        string collationName,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<PostgreSqlCollationCatalogRow>()
            .Where(collation => collation.Name == collationName)
            .OrderBy(collation => collation.Id)
            .Select(collation => collation.ProviderCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // islevi: Identity kolonlarin sahip sequence seed/increment degerlerini tablo/kolon anahtariyla okur.
    private static async Task<Dictionary<(uint RelationId, int ColumnNumber), (string? Seed, string? Increment)>> ReadIdentityValuesByColumnAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        var values = await dbContext.Set<PostgreSqlDependCatalogRow>()
            .Where(dependency => tableIds.Contains(dependency.ReferencedObjectId) &&
                                 dependency.ReferencedObjectSubId > 0 &&
                                 dependency.DependencyType == DatabaseMetadataCatalogConstants.PostgreSql.InternalDependencyType)
            .Join(
                dbContext.Set<PostgreSqlSequenceCatalogRow>(),
                dependency => dependency.ObjectId,
                sequence => sequence.SequenceRelId,
                (dependency, sequence) => new { dependency.ReferencedObjectId, dependency.ReferencedObjectSubId, sequence.StartValue, sequence.Increment })
            .ToListAsync(cancellationToken);

        return values
            .GroupBy(value => (value.ReferencedObjectId, value.ReferencedObjectSubId))
            .ToDictionary(
                group => group.Key,
                group => (
                    Seed: (string?)group.First().StartValue.ToString(CultureInfo.InvariantCulture),
                    Increment: (string?)group.First().Increment.ToString(CultureInfo.InvariantCulture)));
    }

    // islevi: Kolon comment'lerini relation oid + attnum anahtariyla tek batch okur.
    private static async Task<Dictionary<(uint RelationId, int ColumnNumber), string>> ReadColumnCommentsByColumnAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        var pgClassCatalogId = await ReadPgClassCatalogIdAsync(dbContext, cancellationToken);
        if (pgClassCatalogId is null)
        {
            return new Dictionary<(uint RelationId, int ColumnNumber), string>();
        }

        return await dbContext.Set<PostgreSqlDescriptionCatalogRow>()
            .Where(description => description.CatalogId == pgClassCatalogId.Value &&
                                  tableIds.Contains(description.ObjectId) &&
                                  description.SubObjectId > 0)
            .ToDictionaryAsync(
                description => (description.ObjectId, description.SubObjectId),
                description => description.Description,
                cancellationToken);
    }

    // islevi: pg_description satirlarini sinirlamak icin pg_catalog.pg_class katalog relation oid'sini bulur.
    private static Task<uint?> ReadPgClassCatalogIdAsync(
        PostgreSqlCatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<PostgreSqlClassCatalogRow>()
            .Where(relation => relation.Name == DatabaseMetadataCatalogConstants.PostgreSql.PgClassTable)
            .Join(
                dbContext.Set<PostgreSqlNamespaceCatalogRow>()
                    .Where(schema => schema.Name == DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema),
                relation => relation.NamespaceId,
                schema => schema.Id,
                (relation, _) => (uint?)relation.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // islevi: Sema filtresini pg_namespace sorgusuna uygular.
    private static IQueryable<PostgreSqlNamespaceCatalogRow> BuildSchemaQuery(
        PostgreSqlCatalogDbContext dbContext,
        List<string> schemaNames)
    {
        var schemaQuery = dbContext.Set<PostgreSqlNamespaceCatalogRow>();
        return schemaNames.Count > 0
            ? schemaQuery.Where(schema => schemaNames.Contains(schema.Name))
            : schemaQuery.Where(schema =>
                !schema.Name.StartsWith(DatabaseMetadataCatalogConstants.PostgreSql.SystemSchemaPrefix) &&
                schema.Name != DatabaseMetadataCatalogConstants.PostgreSql.InformationSchema);
    }

    // islevi: Secilen semalardan kullanici tablo relation'larini filtreleyen pg_class sorgusunu kurar.
    private static IQueryable<PostgreSqlClassCatalogRow> BuildTableQuery(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> schemaIds)
    {
        return dbContext.Set<PostgreSqlClassCatalogRow>()
            .Where(relation =>
                schemaIds.Contains(relation.NamespaceId) &&
                (relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.TableRelKind ||
                 relation.Kind == DatabaseMetadataCatalogConstants.PostgreSql.PartitionedTableRelKind));
    }

    // islevi: Kolon default ifadelerini tablo/kolon numarasi anahtariyla tek batch okur.
    private static async Task<Dictionary<(uint RelationId, short ColumnNumber), string?>> ReadColumnExpressionsByColumnAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<PostgreSqlAttributeDefaultCatalogRow>()
            .Where(defaultValue => tableIds.Contains(defaultValue.RelationId))
            .Select(defaultValue => new
            {
                defaultValue.RelationId,
                defaultValue.ColumnNumber,
                DefaultValueSql = PostgreSqlCatalogDbContext.GetExpression(defaultValue.BinaryExpression, defaultValue.RelationId)
            })
            .ToDictionaryAsync(
                defaultValue => (defaultValue.RelationId, defaultValue.ColumnNumber),
                defaultValue => defaultValue.DefaultValueSql,
                cancellationToken);
    }

    // islevi: PostgreSQL kolonlarini tip bilgisiyle okuyup tablo kimligine gore gruplanmis snapshot kolonlarina cevirir.
    private async Task<Dictionary<uint, List<SchemaColumnModel>>> ReadColumnsByTableAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        Dictionary<(uint RelationId, short ColumnNumber), string?> expressions,
        Dictionary<(uint RelationId, int ColumnNumber), (string? Seed, string? Increment)> identities,
        Dictionary<(uint RelationId, int ColumnNumber), string> comments,
        CancellationToken cancellationToken)
    {
        var columns = await ReadColumnRowsAsync(dbContext, tableIds, cancellationToken);
        return columns
            .GroupBy(column => column.Column.RelationId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(column => column.Column.ColumnNumber)
                    .Select(column => MapColumn(
                        column.Column,
                        column.TypeName,
                        column.CollationName,
                        expressions.GetValueOrDefault((column.Column.RelationId, column.Column.ColumnNumber)),
                        identities.GetValueOrDefault((column.Column.RelationId, column.Column.ColumnNumber)),
                        comments.GetValueOrDefault((column.Column.RelationId, column.Column.ColumnNumber))))
                    .ToList());
    }

    // islevi: PostgreSQL kolon/type/collation katalog satirlarini tek LINQ sorgusuyla materialize eder.
    private static async Task<List<(PostgreSqlAttributeCatalogRow Column, string TypeName, string? CollationName)>> ReadColumnRowsAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.Set<PostgreSqlAttributeCatalogRow>()
            .Where(column =>
                tableIds.Contains(column.RelationId) &&
                column.ColumnNumber > 0 &&
                !column.IsDropped)
            .Join(
                dbContext.Set<PostgreSqlTypeCatalogRow>(),
                column => column.TypeId,
                type => type.Id,
                (column, type) => new { Column = column, TypeName = type.Name })
            .GroupJoin(
                dbContext.Set<PostgreSqlCollationCatalogRow>(),
                item => item.Column.CollationId,
                collation => collation.Id,
                (item, collations) => new { item.Column, item.TypeName, Collations = collations })
            .SelectMany(
                item => item.Collations.DefaultIfEmpty(),
                (item, collation) => new
                {
                    item.Column,
                    item.TypeName,
                    CollationName = collation == null ? null : collation.Name
                })
            .ToListAsync(cancellationToken);
        return rows.Select(row => (row.Column, row.TypeName, row.CollationName)).ToList();
    }

    // islevi: PostgreSQL index katalog satirlarini okuyup tablo kimligine gore snapshot index listesine cevirir.
    private static async Task<Dictionary<uint, List<SchemaIndexModel>>> ReadIndexesByTableAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        Dictionary<uint, List<SchemaColumnModel>> columnsByTable,
        CancellationToken cancellationToken)
    {
        var indexes = await dbContext.Set<PostgreSqlIndexCatalogRow>()
            .Where(index => tableIds.Contains(index.TableRelId))
            .Join(
                dbContext.Set<PostgreSqlClassCatalogRow>(),
                index => index.IndexRelId,
                indexRelation => indexRelation.Id,
                (index, indexRelation) => new
                {
                    index.TableRelId,
                    index.IndexRelId,
                    Name = indexRelation.Name,
                    index.IsUnique,
                    index.IsPrimary,
                    index.NumberOfKeyColumns,
                    index.ColumnNumbers,
                    FilterDefinition = PostgreSqlCatalogDbContext.GetExpression(index.PredicateExpression, index.TableRelId),
                    Definition = PostgreSqlCatalogDbContext.GetIndexDefinition(index.IndexRelId)
                })
            .ToListAsync(cancellationToken);

        return indexes
            .GroupBy(index => index.TableRelId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(index => index.Name)
                    .Select(index => MapIndex(
                        index.Name,
                        index.IsUnique,
                        index.IsPrimary,
                        ParsePostgreSqlColumnNumbers(index.ColumnNumbers),
                        index.NumberOfKeyColumns,
                        BuildColumnNameByOrdinal(columnsByTable.GetValueOrDefault(index.TableRelId)),
                        index.FilterDefinition,
                        index.Definition))
                    .ToList());
    }

    // islevi: Snapshot kapsamindaki desteklenen PostgreSQL constraint katalog satirlarini okur.
    private static async Task<List<PostgreSqlConstraintCatalogRow>> ReadSupportedConstraintRowsAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<PostgreSqlConstraintCatalogRow>()
            .Where(constraint =>
                tableIds.Contains(constraint.TableRelId) &&
                (
                    constraint.Type == DatabaseMetadataCatalogConstants.PostgreSql.PrimaryKeyConType ||
                    constraint.Type == DatabaseMetadataCatalogConstants.PostgreSql.UniqueConType ||
                    constraint.Type == DatabaseMetadataCatalogConstants.PostgreSql.ForeignKeyConType ||
                    constraint.Type == DatabaseMetadataCatalogConstants.PostgreSql.CheckConType
                ))
            .ToListAsync(cancellationToken);
    }

    // islevi: Constraint definition metinlerini constraint oid anahtariyla tek batch okur.
    private static async Task<Dictionary<uint, string?>> ReadConstraintDefinitionsByIdAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<PostgreSqlConstraintCatalogRow> constraints,
        CancellationToken cancellationToken)
    {
        var constraintIds = constraints.Select(constraint => constraint.Id).ToList();
        if (constraintIds.Count == 0)
        {
            return new Dictionary<uint, string?>();
        }

        return await dbContext.Set<PostgreSqlConstraintCatalogRow>()
            .Where(constraint => constraintIds.Contains(constraint.Id))
            .Select(constraint => new
            {
                constraint.Id,
                Definition = PostgreSqlCatalogDbContext.GetConstraintDefinition(constraint.Id)
            })
            .ToDictionaryAsync(constraint => constraint.Id, constraint => constraint.Definition, cancellationToken);
    }

    // islevi: FK constraint'lerinin referans verdigi tablo oid listesini tekrar etmeyecek sekilde cikarir.
    private static List<uint> GetForeignTableIds(List<PostgreSqlConstraintCatalogRow> constraints)
    {
        return constraints
            .Where(constraint => constraint.ForeignTableRelId != 0)
            .Select(constraint => constraint.ForeignTableRelId)
            .Distinct()
            .ToList();
    }

    // islevi: Constraint katalog satirlarini tablo kimligine gore snapshot constraint listesine cevirir.
    private static Dictionary<uint, List<SchemaConstraintModel>> BuildConstraintsByTable(
        List<PostgreSqlConstraintCatalogRow> constraints,
        Dictionary<uint, string?> definitions,
        Dictionary<uint, string> referencedTableNames,
        Dictionary<(uint TableId, short ColumnNumber), string> columnNames)
    {
        return constraints
            .GroupBy(constraint => constraint.TableRelId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(constraint => MapConstraint(
                        constraint,
                        definitions.GetValueOrDefault(constraint.Id),
                        referencedTableNames,
                        columnNames))
                    .OrderBy(constraint => constraint.TypeCode)
                    .ThenBy(constraint => constraint.Name)
                    .ToList());
    }

    // islevi: Tek PostgreSQL constraint satirini ortak snapshot constraint modeline cevirir.
    internal static SchemaConstraintModel MapConstraint(
        PostgreSqlConstraintCatalogRow constraint,
        string? definition,
        Dictionary<uint, string> referencedTableNames,
        Dictionary<(uint TableId, short ColumnNumber), string> columnNames)
    {
        var model = new SchemaConstraintModel
        {
            Name = constraint.Name,
            TypeCode = MapPostgreSqlConstraintType(constraint.Type),
            Columns = MapPostgreSqlConstraintColumns(constraint.TableRelId, constraint.ColumnNumbers, columnNames),
            Definition = definition,
            IsValidated = constraint.IsValidated,
            IsEnabled = true,
            IsDeferrable = constraint.IsDeferrable,
            IsInitiallyDeferred = constraint.IsInitiallyDeferred
        };
        MapConstraintReference(model, constraint, referencedTableNames, columnNames);
        return model;
    }

    // islevi: PostgreSQL FK hedef ve referential-action alanlarini ortak constraint modeline ekler.
    private static void MapConstraintReference(
        SchemaConstraintModel model,
        PostgreSqlConstraintCatalogRow constraint,
        Dictionary<uint, string> referencedTableNames,
        Dictionary<(uint TableId, short ColumnNumber), string> columnNames)
    {
        model.ReferencedTable = constraint.ForeignTableRelId == 0
            ? null
            : referencedTableNames.GetValueOrDefault(constraint.ForeignTableRelId);
        model.ReferencedColumns = MapPostgreSqlConstraintColumns(
            constraint.ForeignTableRelId,
            constraint.ForeignColumnNumbers,
            columnNames);
        if (constraint.Type != DatabaseMetadataCatalogConstants.PostgreSql.ForeignKeyConType)
        {
            return;
        }

        model.DeleteActionCode = MapPostgreSqlReferentialAction(constraint.DeleteAction);
        model.UpdateActionCode = MapPostgreSqlReferentialAction(constraint.UpdateAction);
    }

    // islevi: PostgreSQL trigger katalog satirlarini okuyup tablo kimligine gore snapshot trigger listesine cevirir.
    private static async Task<Dictionary<uint, List<SchemaTriggerModel>>> ReadTriggersByTableAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        var triggers = await dbContext.Set<PostgreSqlTriggerCatalogRow>()
            .Where(trigger => tableIds.Contains(trigger.RelationId) && !trigger.IsInternal)
            .Select(trigger => new
            {
                trigger.RelationId,
                trigger.Name,
                trigger.EnabledStatus,
                Definition = PostgreSqlCatalogDbContext.GetTriggerDefinition(trigger.Id)
            })
            .ToListAsync(cancellationToken);

        return triggers
            .GroupBy(trigger => trigger.RelationId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(trigger => trigger.Name)
                    .Select(trigger => new SchemaTriggerModel
                    {
                        Name = trigger.Name,
                        Definition = trigger.Definition ?? string.Empty,
                        IsEnabled = trigger.EnabledStatus != DatabaseMetadataCatalogConstants.PostgreSql.DisabledTriggerStatus
                    })
                    .ToList());
    }

    // islevi: pg_attribute + pg_type ham alanlarindan tek bir kanonik kolon modeli uretir; atttypmod'u boyut/precision/scale'e cozer.
    private SchemaColumnModel MapColumn(
        PostgreSqlAttributeCatalogRow column,
        string typeName,
        string? collationName,
        string? expression,
        (string? Seed, string? Increment) identityValues,
        string? comment)
    {
        var shape = ResolvePostgreSqlTypeShape(
            typeName,
            column.TypeModifier);
        var isGenerated = !string.IsNullOrEmpty(column.Generated);
        var model = CreateColumnModel(column, typeName, shape);
        MapColumnDepth(model, column, collationName, expression, identityValues, comment, isGenerated);
        return ApplyTypeMapping(model, typeName);
    }

    // islevi: PostgreSQL kolonunun ad/tip/null/boyut/identity temel alanlarini ortak modele kurar.
    private static SchemaColumnModel CreateColumnModel(
        PostgreSqlAttributeCatalogRow column,
        string typeName,
        DatabaseColumnTypeShapeModel shape)
        => new()
        {
            Name = column.Name,
            Ordinal = column.ColumnNumber,
            RawDataType = BuildRawDataType(typeName, shape.MaxLength, shape.NumericPrecision, shape.NumericScale),
            IsNullable = !column.IsNotNull,
            MaxLength = shape.MaxLength,
            NumericPrecision = shape.NumericPrecision,
            NumericScale = shape.NumericScale,
            IsIdentity = !string.IsNullOrEmpty(column.Identity)
        };

    // islevi: PostgreSQL kolonunun generated/collation/identity-sequence/comment alanlarini ortak modele ekler.
    private static void MapColumnDepth(
        SchemaColumnModel model,
        PostgreSqlAttributeCatalogRow column,
        string? collationName,
        string? expression,
        (string? Seed, string? Increment) identityValues,
        string? comment,
        bool isGenerated)
    {
        model.DefaultValueSql = isGenerated ? null : expression;
        model.CollationName = collationName;
        model.IsGenerated = isGenerated;
        model.GenerationExpression = isGenerated ? expression : null;
        model.IsPersisted = column.Generated == DatabaseMetadataCatalogConstants.PostgreSql.StoredGeneratedKind;
        model.IdentitySeed = identityValues.Seed;
        model.IdentityIncrement = identityValues.Increment;
        model.Comment = comment;
    }

    // islevi: PostgreSQL atttypmod degerini max-length/precision/scale alanlarina cozer.
    private static DatabaseColumnTypeShapeModel ResolvePostgreSqlTypeShape(
        string typeName,
        int typeModifier)
    {
        if (typeModifier <= DatabaseMetadataCatalogConstants.PostgreSql.TypeModifierHeaderSize)
        {
            return new DatabaseColumnTypeShapeModel();
        }

        var packed = typeModifier - DatabaseMetadataCatalogConstants.PostgreSql.TypeModifierHeaderSize;
        return typeName switch
        {
            DatabaseMetadataCatalogConstants.PostgreSql.VarCharTypeName or
                DatabaseMetadataCatalogConstants.PostgreSql.CharTypeName => ResolvePostgreSqlTextShape(packed),
            DatabaseMetadataCatalogConstants.PostgreSql.NumericTypeName => ResolvePostgreSqlDecimalShape(packed),
            _ => new DatabaseColumnTypeShapeModel()
        };
    }

    // islevi: PostgreSQL metin typmod degerini azami karakter uzunluguna cevirir.
    private static DatabaseColumnTypeShapeModel ResolvePostgreSqlTextShape(int packed)
        => new() { MaxLength = packed };

    // islevi: PostgreSQL numeric typmod bitlerini precision ve scale alanlarina ayirir.
    private static DatabaseColumnTypeShapeModel ResolvePostgreSqlDecimalShape(int packed)
        => new()
        {
            NumericPrecision = (packed >> DatabaseMetadataCatalogConstants.PostgreSql.NumericPrecisionShift)
                               & DatabaseMetadataCatalogConstants.PostgreSql.TypeModifierLowWordMask,
            NumericScale = packed & DatabaseMetadataCatalogConstants.PostgreSql.TypeModifierLowWordMask
        };

    // islevi: PostgreSQL index katalog satirini kanonik index modeline cevirir.
    private static SchemaIndexModel MapIndex(
        string name,
        bool isUnique,
        bool isPrimary,
        List<short> columnNumbers,
        short numberOfKeyColumns,
        Dictionary<int, string> columnNameByOrdinal,
        string? filterDefinition,
        string? definition)
    {
        var keyColumnNumbers = columnNumbers.Take(numberOfKeyColumns).ToList();
        var includedColumnNumbers = columnNumbers.Skip(numberOfKeyColumns).ToList();
        return new SchemaIndexModel
        {
            Name = name,
            IsUnique = isUnique,
            IsPrimaryKey = isPrimary,
            Columns = MapPostgreSqlColumnNumbers(keyColumnNumbers, columnNameByOrdinal),
            IncludedColumns = MapPostgreSqlColumnNumbers(includedColumnNumbers, columnNameByOrdinal),
            FilterDefinition = filterDefinition,
            Definition = definition
        };
    }

    // islevi: pg_index.indkey int2vector metnini kolon numarasi listesine cevirir.
    private static List<short> ParsePostgreSqlColumnNumbers(short[] columnNumbers)
    {
        return columnNumbers
            .Where(number => number > 0)
            .ToList();
    }

    // islevi: PostgreSQL kolon numaralarini kolon adlarina cevirir.
    private static List<string> MapPostgreSqlColumnNumbers(
        IEnumerable<short> columnNumbers,
        Dictionary<int, string> columnNameByOrdinal)
    {
        return columnNumbers
            .Select(columnNumber => columnNameByOrdinal.GetValueOrDefault(columnNumber, string.Empty))
            .Where(columnName => !string.IsNullOrEmpty(columnName))
            .ToList();
    }

    // islevi: SchemaColumnModel listesinden kolon numarasi -> kolon adi sozlugu kurar.
    private static Dictionary<int, string> BuildColumnNameByOrdinal(List<SchemaColumnModel>? columns)
        => columns?.ToDictionary(column => column.Ordinal, column => column.Name) ?? new Dictionary<int, string>();

    // islevi: PostgreSQL constraint kolon numaralarini kolon adlarina cevirir.
    private static List<string> MapPostgreSqlConstraintColumns(
        uint tableId,
        short[]? columnNumbers,
        Dictionary<(uint TableId, short ColumnNumber), string> columnNames)
    {
        return (columnNumbers ?? [])
            .Select(columnNumber => columnNames.GetValueOrDefault((tableId, columnNumber), string.Empty))
            .Where(columnName => !string.IsNullOrEmpty(columnName))
            .ToList();
    }

    // islevi: PostgreSQL relation oid listesini schema.table adlarina cevirir.
    private static async Task<Dictionary<uint, string>> GetTableNamesByIdAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Set<PostgreSqlClassCatalogRow>()
            .Where(relation => tableIds.Contains(relation.Id))
            .Join(
                dbContext.Set<PostgreSqlNamespaceCatalogRow>(),
                relation => relation.NamespaceId,
                schema => schema.Id,
                (relation, schema) => new
                {
                    relation.Id,
                    FullName = schema.Name + "." + relation.Name
                })
            .ToDictionaryAsync(table => table.Id, table => table.FullName, cancellationToken);
    }

    // islevi: Tablo/kolon numarasi ciftlerini kolon adlarina ceviren sozlugu tek batch sorguyla kurar.
    private static async Task<Dictionary<(uint TableId, short ColumnNumber), string>> GetColumnNamesByTableAsync(
        PostgreSqlCatalogDbContext dbContext,
        List<uint> tableIds,
        CancellationToken cancellationToken)
    {
        var columns = await dbContext.Set<PostgreSqlAttributeCatalogRow>()
            .Where(column => tableIds.Contains(column.RelationId) &&
                             column.ColumnNumber > 0 &&
                             !column.IsDropped)
            .Select(column => new { column.RelationId, column.ColumnNumber, column.Name })
            .ToListAsync(cancellationToken);

        return columns.ToDictionary(column => (column.RelationId, column.ColumnNumber), column => column.Name);
    }

    // islevi: PostgreSQL constraint tur kodunu ortak snapshot constraint koduna cevirir.
    private static string MapPostgreSqlConstraintType(string type)
        => type switch
        {
            DatabaseMetadataCatalogConstants.PostgreSql.PrimaryKeyConType => SchemaConstraintTypeCodes.PrimaryKey,
            DatabaseMetadataCatalogConstants.PostgreSql.UniqueConType => SchemaConstraintTypeCodes.Unique,
            DatabaseMetadataCatalogConstants.PostgreSql.ForeignKeyConType => SchemaConstraintTypeCodes.ForeignKey,
            DatabaseMetadataCatalogConstants.PostgreSql.CheckConType => SchemaConstraintTypeCodes.Check,
            _ => type
        };

    // islevi: PostgreSQL FK davranis karakter kodunu ortak snapshot koduna cevirir.
    private static string MapPostgreSqlReferentialAction(string action)
        => action switch
        {
            DatabaseMetadataCatalogConstants.PostgreSql.NoActionReferentialAction => SchemaReferentialActionCodes.NoAction,
            DatabaseMetadataCatalogConstants.PostgreSql.RestrictReferentialAction => SchemaReferentialActionCodes.Restrict,
            DatabaseMetadataCatalogConstants.PostgreSql.CascadeReferentialAction => SchemaReferentialActionCodes.Cascade,
            DatabaseMetadataCatalogConstants.PostgreSql.SetNullReferentialAction => SchemaReferentialActionCodes.SetNull,
            DatabaseMetadataCatalogConstants.PostgreSql.SetDefaultReferentialAction => SchemaReferentialActionCodes.SetDefault,
            _ => SchemaReferentialActionCodes.Unknown
        };
}
