using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Secili tablolarin kaynak/hedef COUNT(*) sonuclarini karsilastirip veri finding'leri uretir.
// sistemdeki gorevi: T7 row-count yon ve fark karari tek yerde kalir; provider repository'ler yalniz kesin sayim okur.
public class DataRowCountComparisonManager : ITransientDependency
{
    // islevi: Esit sayimli tablolari elemek, farkli sayimli tablolari yon bilgisiyle DataDifferenceModel'e cevirir.
    public List<DataDifferenceModel> Compare(
        List<ComparisonTableIdentifierModel> tables,
        List<TableRowCountModel> sourceRowCounts,
        List<TableRowCountModel> targetRowCounts)
    {
        var sourceByTable = BuildRowCountByTable(sourceRowCounts);
        var targetByTable = BuildRowCountByTable(targetRowCounts);
        var differences = new List<DataDifferenceModel>();

        foreach (var table in SortTables(tables))
        {
            var key = BuildTableKey(table.SchemaName, table.TableName);
            var hasSource = sourceByTable.TryGetValue(key, out var sourceRowCount);
            var hasTarget = targetByTable.TryGetValue(key, out var targetRowCount);

            if (!ShouldCreateDifference(hasSource, hasTarget, sourceRowCount, targetRowCount))
            {
                continue;
            }

            differences.Add(CreateDifference(table, hasSource, hasTarget, sourceRowCount, targetRowCount));
        }

        return differences;
    }

    // islevi: Row count sonuclarini sema+tablo anahtarina gore tekil sozluge cevirir.
    private static Dictionary<string, TableRowCountModel> BuildRowCountByTable(List<TableRowCountModel> rowCounts)
    {
        return rowCounts
            .GroupBy(rowCount => BuildTableKey(rowCount.SchemaName, rowCount.TableName), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    // islevi: Tablolari response ve testlerde kararli sira ile gezer.
    private static List<ComparisonTableIdentifierModel> SortTables(List<ComparisonTableIdentifierModel> tables)
    {
        return tables
            .GroupBy(table => BuildTableKey(table.SchemaName, table.TableName), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(table => table.SchemaName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(table => table.TableName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // islevi: Iki taraf da sayildi ve esit ise bulgu uretmez; eksik veya farkli sayim varsa raporlanir.
    private static bool ShouldCreateDifference(
        bool hasSource,
        bool hasTarget,
        TableRowCountModel? sourceRowCount,
        TableRowCountModel? targetRowCount)
    {
        if (!hasSource || !hasTarget)
        {
            return hasSource || hasTarget;
        }

        return sourceRowCount!.RowCount != targetRowCount!.RowCount;
    }

    // islevi: Count farkini API'nin okuyacagi yon kodu ve signed fark degeriyle modele tasir.
    private static DataDifferenceModel CreateDifference(
        ComparisonTableIdentifierModel table,
        bool hasSource,
        bool hasTarget,
        TableRowCountModel? sourceRowCount,
        TableRowCountModel? targetRowCount)
    {
        var sourceCount = hasSource ? sourceRowCount!.RowCount : (long?)null;
        var targetCount = hasTarget ? targetRowCount!.RowCount : (long?)null;
        return new DataDifferenceModel
        {
            SchemaName = table.SchemaName,
            TableName = table.TableName,
            KindCode = GetKindCode(sourceCount, targetCount),
            SourceRowCount = sourceCount,
            TargetRowCount = targetCount,
            RowCountDifference = GetRowCountDifference(sourceCount, targetCount)
        };
    }

    // islevi: Row-count sonucundaki fazlaligin hangi tarafta oldugunu DifferenceKindCodes ile ifade eder.
    private static string GetKindCode(long? sourceCount, long? targetCount)
    {
        if (!targetCount.HasValue || sourceCount > targetCount)
        {
            return DifferenceKindCodes.OnlyInSource;
        }

        if (!sourceCount.HasValue || targetCount > sourceCount)
        {
            return DifferenceKindCodes.OnlyInTarget;
        }

        return DifferenceKindCodes.Modified;
    }

    // islevi: Kaynak eksi hedef signed farkini uretir; taraflardan biri sayilamadiysa null kalir.
    private static long? GetRowCountDifference(long? sourceCount, long? targetCount)
        => sourceCount.HasValue && targetCount.HasValue
            ? sourceCount.Value - targetCount.Value
            : null;

    // islevi: Sema+tablo kimligini case-insensitive sozluk icin kararli anahtara cevirir.
    private static string BuildTableKey(string schemaName, string tableName)
        => string.Join(ComparisonCanonicalTextConstants.KeySeparator, schemaName.Trim(), tableName.Trim());
}
