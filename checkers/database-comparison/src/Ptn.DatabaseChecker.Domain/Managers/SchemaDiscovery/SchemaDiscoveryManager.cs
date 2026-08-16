using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using System;
using System.Linq;
using System.Threading;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.SchemaDiscovery;

// islevi: Kayitli bir baglanti uzerinden hedef veritabaninin semalarini/nesnelerini/tam fotografini motor-ozel okuyucuyla kesfeder.
// sistemdeki gorevi: "secret coz + motor sec + katalog oku" ara akisini AppService'ten uzak tutar; cagiran yalniz motor-bagimsiz modelleri gorur. Baglanti CRUD kurallari DatabaseConnectionManager'da kalir, bu servis yalnizca canli hedef okumasi yapar.
public class SchemaDiscoveryManager : DomainService
{
    // Entity -> secret cozulmus baglanti modeli; baglanti test'iyle paylasilan tek kaynak.
    private DatabaseConnectionInfoFactory ConnectionInfoFactory
        => LazyServiceProvider.LazyGetRequiredService<DatabaseConnectionInfoFactory>();

    // Motor koduna uygun sema okuyucusunu secer (acik/kapali).
    private IEngineComponentResolver<IDatabaseSchemaDiscoveryRepository> RepositoryResolver
        => LazyServiceProvider.LazyGetRequiredService<IEngineComponentResolver<IDatabaseSchemaDiscoveryRepository>>();

    // Fotografi kanonik Merkle muhrune indiren saf hesaplayici; I/O ve saat sahibi burasidir, hesaplayici degil.
    private SchemaFingerprintCalculator FingerprintCalculator
        => LazyServiceProvider.LazyGetRequiredService<SchemaFingerprintCalculator>();

    // Hedefli tablo fotografini kararli lint uyari kodlarina indiren domain siniflandiricisi.
    private DatabaseLintManager LintManager
        => LazyServiceProvider.LazyGetRequiredService<DatabaseLintManager>();

    // islevi: Baglantidaki kullanici semalarini hafif listeler (T4 kesif 1. adim).
    public Task<List<DatabaseSchemaModel>> GetSchemasAsync(DatabaseConnection connection)
        => GetSchemasAsync(connection, default);

    public async Task<List<DatabaseSchemaModel>> GetSchemasAsync(
        DatabaseConnection connection,
        CancellationToken cancellationToken)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        return await repository.GetSchemasAsync(info, cancellationToken);
    }

    // islevi: Verilen semadaki nesneleri (tablo/view/trigger/function/procedure) hafif listeler (T4 kesif 2. adim).
    public Task<List<DatabaseSchemaObjectModel>> GetObjectsAsync(
        DatabaseConnection connection,
        string schemaName)
        => GetObjectsAsync(connection, schemaName, default);

    public async Task<List<DatabaseSchemaObjectModel>> GetObjectsAsync(
        DatabaseConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        return await repository.GetObjectsAsync(info, schemaName, cancellationToken);
    }

    // islevi: Verilen semalarin (bos ise tum kullanici semalari) tam sema fotografini okur; karsilastirma motoru yalniz motor-bagimsiz SchemaSnapshotModel'i gorur.
    public virtual Task<SchemaSnapshotModel> ReadSnapshotAsync(
        DatabaseConnection connection,
        List<string> schemaNames)
        => ReadSnapshotAsync(connection, schemaNames, default);

    public virtual async Task<SchemaSnapshotModel> ReadSnapshotAsync(
        DatabaseConnection connection,
        List<string> schemaNames,
        CancellationToken cancellationToken)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        return await repository.ReadSnapshotAsync(info, schemaNames, cancellationToken);
    }

    // islevi: Istenen semalari katalogda dogrular, fotografi okur ve kanonik Merkle muhrunu uretir; fotograf ne hedefe ne de yerel tabloya yazilir.
    public virtual async Task<SchemaFingerprintModel> ReadFingerprintAsync(
        DatabaseConnection connection,
        List<string> schemaNames,
        CancellationToken cancellationToken = default)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        await GuardCatalogSchemasAsync(repository, info, schemaNames, cancellationToken);
        var snapshot = await repository.ReadSnapshotAsync(info, schemaNames, cancellationToken);
        var fingerprint = FingerprintCalculator.Calculate(snapshot);
        fingerprint.ComputedAt = Clock.Now;
        return fingerprint;
    }

    // islevi: Istenen her sema adinin hedefin kullanici sema katalogunda bulundugunu okuma oncesinde dogrular.
    // sistemdeki gorevi: Katalogda olmayan sema sessizce atlanip eksik ama gecerli gorunen bir muhur uretilmesini engeller; bos liste mevcut "tum kullanici semalari" sozlesmesini korur.
    private static async Task GuardCatalogSchemasAsync(
        IDatabaseSchemaDiscoveryRepository repository,
        DatabaseConnectionInfo info,
        List<string> schemaNames,
        CancellationToken cancellationToken)
    {
        if (schemaNames.Count == 0)
        {
            return;
        }

        var catalog = await repository.GetSchemasAsync(info, cancellationToken);
        var known = catalog.Select(schema => schema.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (schemaNames.Any(name => !known.Contains(name)))
        {
            throw new BusinessException(ComparisonExceptionCodes.SchemaNotFound);
        }
    }

    // islevi: Motor destekliyorsa tek server setting degerini mevcut schema repository katalog LINQ'una delege eder.
    public async Task<string?> ReadSettingAsync(
        DatabaseConnection connection,
        string settingName,
        CancellationToken cancellationToken = default)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        return await repository.ReadSettingAsync(info, settingName, cancellationToken);
    }

    // islevi: Maliyetli tam snapshot'i yalniz senaryo yaziminda tek tablo ve bir-seviye FK komsuluk ozetine indirger.
    public async Task<TableDescriptionModel> DescribeTableAsync(
        DatabaseConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        var snapshot = await repository.ReadSnapshotAsync(
            info, new List<string> { schemaName }, tableName, cancellationToken);
        var table = snapshot.Tables.FirstOrDefault(candidate =>
            string.Equals(candidate.Schema, schemaName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.Name, tableName, StringComparison.OrdinalIgnoreCase));
        if (table is null)
        {
            throw new BusinessException(AssertionExceptionCodes.TableNotFound);
        }

        return BuildLintedTableDescription(snapshot, table);
    }

    // islevi: Birden cok assertion hedefini tek snapshot round-trip'inde dar tablo tanimlarina indirger.
    public virtual async Task<List<TableDescriptionModel>> DescribeTablesAsync(
        DatabaseConnection connection,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken = default)
    {
        if (tables.Count == 0)
        {
            return [];
        }

        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        var schemas = tables.Select(table => table.SchemaName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var requested = tables.Select(table =>
                FindingAddressGrammar.FormatTargetAddress(table.SchemaName, table.TableName, null))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var snapshot = await repository.ReadSnapshotAsync(info, schemas, cancellationToken);
        return snapshot.Tables
            .Where(table => requested.Contains(
                FindingAddressGrammar.FormatTargetAddress(table.Schema, table.Name, null)))
            .Select(table => BuildTableDescription(snapshot, table))
            .ToList();
    }

    // islevi: Dar tablo ozetine ayni katalog fotografindan turetilen lint uyarilarini ekler.
    private TableDescriptionModel BuildLintedTableDescription(
        SchemaSnapshotModel snapshot,
        SchemaTableModel table)
    {
        var description = BuildTableDescription(snapshot, table);
        description.LintWarnings = LintManager.Evaluate(table);
        return description;
    }

    // islevi: Tam snapshot'taki hedef tabloyu DescribeTable'in dar domain modeline cevirir.
    internal static TableDescriptionModel BuildTableDescription(
        SchemaSnapshotModel snapshot,
        SchemaTableModel table)
        => new()
        {
            SchemaName = table.Schema,
            TableName = table.Name,
            Columns = table.Columns.Select(MapDescriptionColumn).ToList(),
            PrimaryKey = ResolvePrimaryKey(table),
            UniqueIndexes = ResolveUniqueIndexes(table),
            ForeignKeyNeighbors = ResolveForeignKeyNeighbors(snapshot, table)
        };

    // islevi: Schema kolonunu ad, kanonik tip ve nullability ozetine indirger.
    private static TableDescriptionColumnModel MapDescriptionColumn(SchemaColumnModel column)
        => new()
        {
            Name = column.Name,
            CanonicalDataTypeCode = column.CanonicalDataType ?? CanonicalDataTypeCodes.Unknown,
            IsNullable = column.IsNullable
        };

    // islevi: Hedef tablonun primary key index'ini DescribeTable anahtar modeline cevirir.
    private static TableKeyDefinitionModel? ResolvePrimaryKey(SchemaTableModel table)
    {
        var index = table.Indexes.FirstOrDefault(candidate => candidate.IsPrimaryKey);
        return index is null ? null : MapKey(index);
    }

    // islevi: Hedef tablonun filtresiz unique index'lerini senaryo anahtari olarak listeler.
    private static List<TableKeyDefinitionModel> ResolveUniqueIndexes(SchemaTableModel table)
        => table.Indexes
            .Where(index => index.IsUnique && !index.IsPrimaryKey && string.IsNullOrWhiteSpace(index.FilterDefinition))
            .Select(MapKey)
            .ToList();

    // islevi: Schema index'ini ad ve sirali kolonlardan olusan anahtar ozetine cevirir.
    private static TableKeyDefinitionModel MapKey(SchemaIndexModel index)
        => new() { Name = index.Name, Columns = index.Columns.ToList() };

    // islevi: Hedef tablonun disari ve ayni snapshot icindeki iceri FK komsularini tek seviyede toplar.
    private static List<ForeignKeyNeighborModel> ResolveForeignKeyNeighbors(
        SchemaSnapshotModel snapshot,
        SchemaTableModel table)
    {
        var neighbors = ResolveOutgoingNeighbors(table);
        neighbors.AddRange(snapshot.Tables
            .Where(candidate => !ReferenceEquals(candidate, table))
            .SelectMany(candidate => ResolveIncomingNeighbors(candidate, table)));
        return neighbors;
    }

    // islevi: Hedef tablodan baska tabloya giden FK'leri komsu modellerine cevirir.
    private static List<ForeignKeyNeighborModel> ResolveOutgoingNeighbors(SchemaTableModel table)
        => table.Constraints
            .Where(constraint => constraint.TypeCode == SchemaConstraintTypeCodes.ForeignKey)
            .Select(constraint => MapOutgoingNeighbor(table.Schema, constraint))
            .Where(neighbor => neighbor is not null)
            .Cast<ForeignKeyNeighborModel>()
            .ToList();

    // islevi: Snapshot'taki baska bir tablodan hedef tabloya gelen FK'leri komsu modellerine cevirir.
    private static IEnumerable<ForeignKeyNeighborModel> ResolveIncomingNeighbors(
        SchemaTableModel candidate,
        SchemaTableModel target)
        => candidate.Constraints
            .Where(constraint => constraint.TypeCode == SchemaConstraintTypeCodes.ForeignKey &&
                                 ReferencesTable(constraint.ReferencedTable, target))
            .Select(constraint => new ForeignKeyNeighborModel
            {
                DirectionCode = ForeignKeyDirectionCodes.Incoming,
                ConstraintName = constraint.Name,
                SchemaName = candidate.Schema,
                TableName = candidate.Name,
                LocalColumns = constraint.ReferencedColumns.ToList(),
                NeighborColumns = constraint.Columns.ToList()
            });

    // islevi: Disari FK'nin hedef tablo adini ayni sema varsayimiyla komsu modeline ayirir.
    private static ForeignKeyNeighborModel? MapOutgoingNeighbor(
        string currentSchema,
        SchemaConstraintModel constraint)
    {
        if (string.IsNullOrWhiteSpace(constraint.ReferencedTable))
        {
            return null;
        }

        var (schema, table) = SplitTableAddress(currentSchema, constraint.ReferencedTable);
        return new ForeignKeyNeighborModel
        {
            DirectionCode = ForeignKeyDirectionCodes.Outgoing,
            ConstraintName = constraint.Name,
            SchemaName = schema,
            TableName = table,
            LocalColumns = constraint.Columns.ToList(),
            NeighborColumns = constraint.ReferencedColumns.ToList()
        };
    }

    // islevi: Provider snapshot'indaki yalniz-ad veya schema.table FK adresini iki parcaya ayirir.
    private static (string Schema, string Table) SplitTableAddress(string defaultSchema, string address)
    {
        var separatorIndex = address.IndexOf('.');
        return separatorIndex < 0
            ? (defaultSchema, address)
            : (address[..separatorIndex], address[(separatorIndex + 1)..]);
    }

    // islevi: FK hedef adresinin verilen tabloyu case-insensitive isaret edip etmedigini bildirir.
    private static bool ReferencesTable(string? referencedTable, SchemaTableModel target)
    {
        if (string.IsNullOrWhiteSpace(referencedTable))
        {
            return false;
        }

        var (schema, table) = SplitTableAddress(target.Schema, referencedTable);
        return string.Equals(schema, target.Schema, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(table, target.Name, StringComparison.OrdinalIgnoreCase);
    }
}
