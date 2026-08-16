using Ptn.DatabaseChecker.Constants.Capabilities;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Interface.Capabilities;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Capabilities;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Correlation;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Repository.Capabilities;

// islevi: Logical decoding yokken mevcut row-count ve table-data manager'lariyla capture oncesi/sonrasi farki cikarir.
// sistemdeki gorevi: Yeni comparison motoru veya SQL yazmadan inferred advisory footprint ureten fallback repository'sidir.
public class DiffWriteSetRepository : IDiffWriteSetRepository, ITransientDependency
{
    private readonly DatabaseDataComparisonManager _dataManager;
    private readonly DataRowCountComparisonManager _rowCountManager;
    private readonly TableDataComparisonManager _tableDataManager;

    // islevi: Diff fallback'i mevcut veri okuma ve comparison manager sahipleriyle kurar.
    public DiffWriteSetRepository(
        DatabaseDataComparisonManager dataManager,
        DataRowCountComparisonManager rowCountManager,
        TableDataComparisonManager tableDataManager)
    {
        _dataManager = dataManager;
        _rowCountManager = rowCountManager;
        _tableDataManager = tableDataManager;
    }

    // islevi: Aday tablolarin before/after count ve snapshotlarini mevcut motorlarla karsilastirir.
    public async Task<WriteSetResult> CaptureAsync(
        DatabaseConnection connection,
        List<ComparisonTableIdentifierModel> candidateTables,
        CorrelationRef? correlation,
        CancellationToken cancellationToken = default)
    {
        var beforeCounts = await _dataManager.ReadRowCountsAsync(connection, candidateTables);
        var beforeTables = await _dataManager.ReadTableDataAsync(connection, candidateTables);
        await Task.Delay(WriteSetConsts.CaptureTimeoutMilliseconds, cancellationToken);
        var afterCounts = await _dataManager.ReadRowCountsAsync(connection, candidateTables);
        var afterTables = await _dataManager.ReadTableDataAsync(connection, candidateTables);
        var countDifferences = _rowCountManager.Compare(candidateTables, beforeCounts, afterCounts);
        var dataDifferences = _tableDataManager.Compare(candidateTables, beforeTables, afterTables);
        return BuildResult(countDifferences, dataDifferences, beforeCounts, afterCounts);
    }

    // islevi: Mevcut manager bulgularini inferred tablo, kolon ve row-delta ozetine cevirir.
    private static WriteSetResult BuildResult(
        List<DataDifferenceModel> countDifferences,
        List<DataDifferenceModel> dataDifferences,
        List<TableRowCountModel> beforeCounts,
        List<TableRowCountModel> afterCounts)
    {
        var tables = countDifferences.Concat(dataDifferences)
            .Select(difference => FormatTable(difference.SchemaName, difference.TableName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return new WriteSetResult
        {
            StrengthCode = FootprintStrengthCodes.Inferred,
            Tables = tables,
            Columns = BuildColumnRefs(dataDifferences),
            RowDeltas = BuildRowDeltas(tables, beforeCounts, afterCounts),
            IsAdvisoryOnly = true
        };
    }

    // islevi: Cell-level mevcut manager bulgularini schema.table.column referanslarina indirger.
    private static List<string> BuildColumnRefs(IEnumerable<DataDifferenceModel> differences)
        => differences.SelectMany(difference => difference.RowDifferences
                .SelectMany(row => row.ValueDifferences
                    .Select(value =>
                        $"{FormatTable(difference.SchemaName, difference.TableName)}.{value.ColumnName}")))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

    // islevi: Degisen tablolarin before/after count'larini signed advisory delta item'larina cevirir.
    private static List<WriteSetTableDelta> BuildRowDeltas(
        IEnumerable<string> tables,
        IEnumerable<TableRowCountModel> beforeCounts,
        IEnumerable<TableRowCountModel> afterCounts)
    {
        var before = BuildRowCountMap(beforeCounts);
        var after = BuildRowCountMap(afterCounts);
        return tables.Select(table => CreateRowDelta(table, before, after)).ToList();
    }

    // islevi: Row count listesini schema.table anahtarina gore case-insensitive sozluge cevirir.
    private static Dictionary<string, long> BuildRowCountMap(IEnumerable<TableRowCountModel> rowCounts)
        => rowCounts.ToDictionary(
            item => FormatTable(item.SchemaName, item.TableName),
            item => item.RowCount,
            StringComparer.OrdinalIgnoreCase);

    // islevi: Tek tablo icin after-before signed row delta item'ini kurar.
    private static WriteSetTableDelta CreateRowDelta(
        string table,
        IReadOnlyDictionary<string, long> before,
        IReadOnlyDictionary<string, long> after)
    {
        var beforeCount = before.GetValueOrDefault(table);
        var afterCount = after.GetValueOrDefault(table);
        return new WriteSetTableDelta
        {
            Table = table,
            BeforeRowCount = beforeCount,
            AfterRowCount = afterCount,
            Delta = afterCount - beforeCount
        };
    }

    // islevi: Sema ve tabloyu result adresi icin kararli schema.table formuna cevirir.
    private static string FormatTable(string schemaName, string tableName)
        => $"{schemaName}.{tableName}";
}
