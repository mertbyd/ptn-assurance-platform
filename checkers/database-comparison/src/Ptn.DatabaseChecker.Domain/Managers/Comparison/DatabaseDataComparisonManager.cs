using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Settings;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Kayitli bir baglanti uzerinden migration history, tablo yapisi, kesin sayim ve satir verisi okumalarini motor-ozel repository'ye delege eder.
// sistemdeki gorevi: "secret coz + engine sec + satir limitini uygula + batch oku" ara akisini AppService ve comparison manager'larindan uzak tutar.
public class DatabaseDataComparisonManager : DomainService
{
    // Entity -> secret cozulmus runtime baglanti modeli; schema discovery ve connection test ile ayni tek kaynak kullanilir.
    private DatabaseConnectionInfoFactory ConnectionInfoFactory
        => LazyServiceProvider.LazyGetRequiredService<DatabaseConnectionInfoFactory>();

    // Motor koduna uygun veri/migration okuyucusunu secer.
    private IEngineComponentResolver<IDatabaseDataComparisonRepository> RepositoryResolver
        => LazyServiceProvider.LazyGetRequiredService<IEngineComponentResolver<IDatabaseDataComparisonRepository>>();

    // Motor koduna uygun schema snapshot okuyucusunu secer; assertion yapisi mevcut katalog omurgasindan gelir.
    private IEngineComponentResolver<IDatabaseSchemaDiscoveryRepository> SchemaRepositoryResolver
        => LazyServiceProvider.LazyGetRequiredService<IEngineComponentResolver<IDatabaseSchemaDiscoveryRepository>>();

    // Tenant -> global -> default fallback zincirinden exact row/cell limitini okur.
    private ISettingProvider SettingProvider
        => LazyServiceProvider.LazyGetRequiredService<ISettingProvider>();

    // islevi: Baglantidaki EF migration defterini okur; defter yoksa provider bos liste dondurur.
    public async Task<List<MigrationHistoryEntryModel>> ReadMigrationHistoryAsync(DatabaseConnection connection)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        return await repository.ReadMigrationHistoryAsync(info);
    }

    // islevi: Snapshot/scope ile secilmis tablolarda kesin row-count okumasini yapar.
    public async Task<List<TableRowCountModel>> ReadRowCountsAsync(
        DatabaseConnection connection,
        List<ComparisonTableIdentifierModel> tables)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        return await repository.ReadRowCountsAsync(info, tables);
    }

    // islevi: Secili tablolari katalogda batch dogrular, exact count limitini uygular ve tum satirlari tek batch okur.
    // sistemdeki gorevi: Secret/engine secimi ile runtime row-limit kararini tek manager'da tutar; provider SQL'i repository'de kalir.
    public async Task<List<TableDataSnapshotModel>> ReadTableDataAsync(
        DatabaseConnection connection,
        List<ComparisonTableIdentifierModel> tables)
    {
        if (tables.Count == 0)
        {
            return new List<TableDataSnapshotModel>();
        }

        var info = await ConnectionInfoFactory.BuildAsync(connection);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        var structures = await repository.ReadTableStructuresAsync(info, tables);
        if (structures.Count == 0)
        {
            return new List<TableDataSnapshotModel>();
        }

        var existingTables = structures
            .Select(structure => new ComparisonTableIdentifierModel
            {
                SchemaName = structure.SchemaName,
                TableName = structure.TableName
            })
            .ToList();
        var rowCounts = await repository.ReadRowCountsAsync(info, existingTables);
        await EnsureRowLimitAsync(rowCounts);

        return await repository.ReadTableDataAsync(info, structures);
    }

    // islevi: Assertion hedef tablosunu tam schema snapshot'tan bulup kolon tipi ve unique anahtar yapisina indirger.
    public virtual async Task<TableDataStructureModel?> ResolveAssertionStructureAsync(
        DatabaseConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = SchemaRepositoryResolver.Resolve(connection.Engine.Code);
        var snapshot = await repository.ReadSnapshotAsync(
            info, new List<string> { schemaName }, tableName, cancellationToken);
        var table = snapshot.Tables.FirstOrDefault(candidate =>
            string.Equals(candidate.Schema, schemaName, System.StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.Name, tableName, System.StringComparison.OrdinalIgnoreCase));
        return table is null ? null : BuildAssertionStructure(table);
    }

    // islevi: Tek assertion denemesinde kesin count ve gerekliyse sinirli satir verisini ayni repository bileseninden okur.
    public virtual async Task<RowAssertionObservation> ReadAssertionObservationAsync(
        DatabaseConnection connection,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        int maxRows,
        bool includeRows,
        CancellationToken cancellationToken = default)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        return includeRows
            ? await ReadRowsObservationAsync(
                repository, info, structure, keyValues, maxRows, cancellationToken)
            : await ReadCountObservationAsync(
                repository, info, structure, keyValues, cancellationToken);
    }

    // islevi: Unique anahtarli row assertion'ini tek hedef round-trip'te sinirli satir ve satir sayisina cevirir.
    private static async Task<RowAssertionObservation> ReadRowsObservationAsync(
        IDatabaseDataComparisonRepository repository,
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        int maxRows,
        CancellationToken cancellationToken)
    {
        var rows = await repository.ReadRowsByKeyAsync(
            info, structure, keyValues, maxRows, cancellationToken);
        return new RowAssertionObservation { RowCount = rows.Count, Rows = rows };
    }

    // islevi: Count/absence assertion'ini satir payload'i okumadan tek kesin sayim sonucuna cevirir.
    private static async Task<RowAssertionObservation> ReadCountObservationAsync(
        IDatabaseDataComparisonRepository repository,
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        CancellationToken cancellationToken)
        => new()
        {
            RowCount = await repository.CountByKeyAsync(
                info, structure, keyValues, cancellationToken)
        };

    // islevi: Schema table modelini assertion sorgusu ve matcher icin gereken dar yapisal modele cevirir.
    private static TableDataStructureModel BuildAssertionStructure(SchemaTableModel table)
    {
        return new TableDataStructureModel
        {
            SchemaName = table.Schema,
            TableName = table.Name,
            ColumnNames = table.Columns.Select(column => column.Name).ToList(),
            PrimaryKeyColumns = ResolvePrimaryKeyColumns(table),
            Columns = table.Columns.Select(BuildAssertionColumn).ToList(),
            UniqueKeyColumnSets = ResolveUniqueKeys(table)
        };
    }

    // islevi: Schema kolonunu assertion matcher'inin kanonik tip modeline indirger.
    private static TableDataColumnModel BuildAssertionColumn(SchemaColumnModel column)
        => new()
        {
            Name = column.Name,
            CanonicalDataTypeCode = column.CanonicalDataType ?? CanonicalDataTypeCodes.Unknown,
            NumericScale = column.NumericScale
        };

    // islevi: Primary key kolonlarini index veya constraint tanimindan kararli oncelikle cozer.
    private static List<string> ResolvePrimaryKeyColumns(SchemaTableModel table)
        => table.Indexes.FirstOrDefault(index => index.IsPrimaryKey)?.Columns.ToList()
           ?? table.Constraints.FirstOrDefault(constraint =>
               constraint.TypeCode == SchemaConstraintTypeCodes.PrimaryKey)?.Columns.ToList()
           ?? new List<string>();

    // islevi: PK ile filtresiz unique index/constraint kolon kumelerini tekrar etmeden toplar.
    private static List<List<string>> ResolveUniqueKeys(SchemaTableModel table)
    {
        var keys = table.Indexes
            .Where(index => (index.IsPrimaryKey || index.IsUnique) && string.IsNullOrWhiteSpace(index.FilterDefinition))
            .Select(index => index.Columns.ToList())
            .Concat(table.Constraints
                .Where(constraint => constraint.TypeCode is SchemaConstraintTypeCodes.PrimaryKey or SchemaConstraintTypeCodes.Unique)
                .Select(constraint => constraint.Columns.ToList()));
        return keys
            .Where(key => key.Count > 0)
            .GroupBy(BuildColumnSetKey, System.StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    // islevi: Kolon kumesini order-insensitive duplicate eleme anahtarina cevirir.
    private static string BuildColumnSetKey(List<string> columns)
        => string.Join("|", columns.OrderBy(column => column, System.StringComparer.OrdinalIgnoreCase));

    // islevi: Exact karsilastirmaya alinacak her tablonun tenant row limitini asmadigini dogrular.
    private async Task EnsureRowLimitAsync(List<TableRowCountModel> rowCounts)
    {
        var maxRowsPerTable = await SettingProvider.GetAsync(
            DatabaseCheckerSettings.DataComparison.MaxRowsPerTable,
            DatabaseCheckerSettings.DataComparison.DefaultMaxRowsPerTable);
        if (rowCounts.Any(rowCount => rowCount.RowCount > maxRowsPerTable))
        {
            throw new BusinessException(DataComparisonExceptionCodes.RowLimitExceeded)
                .WithData(nameof(DatabaseCheckerSettings.DataComparison.MaxRowsPerTable), maxRowsPerTable);
        }
    }
}
