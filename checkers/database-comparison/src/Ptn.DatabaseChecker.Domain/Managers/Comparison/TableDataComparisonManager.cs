using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Secili tablo fotograflarini PK bazinda eslestirip table hash, row direction ve cell-level farklari uretir.
// sistemdeki gorevi: Provider JSON/SQL ayrintilarindan bagimsiz saf T7/T8 data diff motorudur; PK olmayan tabloda exact row-hash multiset karsilastirmasina duser.
public class TableDataComparisonManager : ITransientDependency
{
    private static readonly FindingValueRedactor ValueRedactor = new();

    // islevi: 0.1.x veri karsilastirma imzasini guvenli varsayilan saklama politikasiyla korur.
    public List<DataDifferenceModel> Compare(
        List<ComparisonTableIdentifierModel> requestedTables,
        List<TableDataSnapshotModel> sourceTables,
        List<TableDataSnapshotModel> targetTables)
        => Compare(
            requestedTables,
            sourceTables,
            targetTables,
            new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty));

    // islevi: Istenen tablo sirasi uzerinden iki tarafin exact data snapshot'larini karsilastirir.
    public List<DataDifferenceModel> Compare(
        List<ComparisonTableIdentifierModel> requestedTables,
        List<TableDataSnapshotModel> sourceTables,
        List<TableDataSnapshotModel> targetTables,
        ValueRetentionPolicy retentionPolicy)
    {
        var sourceByTable = BuildTableByKey(sourceTables);
        var targetByTable = BuildTableByKey(targetTables);
        var differences = new List<DataDifferenceModel>();

        foreach (var requestedTable in requestedTables)
        {
            var key = BuildTableKey(requestedTable.SchemaName, requestedTable.TableName);
            sourceByTable.TryGetValue(key, out var sourceTable);
            targetByTable.TryGetValue(key, out var targetTable);
            var difference = CompareTable(requestedTable, sourceTable, targetTable, retentionPolicy);
            if (difference is not null)
            {
                differences.Add(difference);
            }
        }

        return differences;
    }

    // islevi: Tek tablonun hash ve row/cell farklarini hesaplar; iki taraf da yoksa veya hash esitse bulgu uretmez.
    private static DataDifferenceModel? CompareTable(
        ComparisonTableIdentifierModel requestedTable,
        TableDataSnapshotModel? sourceTable,
        TableDataSnapshotModel? targetTable,
        ValueRetentionPolicy retentionPolicy)
    {
        if (sourceTable is null && targetTable is null)
        {
            return null;
        }

        var sourceHash = ComputeTableHash(sourceTable);
        var targetHash = ComputeTableHash(targetTable);
        if (sourceTable is not null &&
            targetTable is not null &&
            string.Equals(sourceHash, targetHash, StringComparison.Ordinal))
        {
            return null;
        }

        var primaryKeyColumns = ResolveComparablePrimaryKey(sourceTable, targetTable);
        var rowDifferences = primaryKeyColumns.Count > 0
            ? CompareRowsByPrimaryKey(sourceTable, targetTable, primaryKeyColumns, retentionPolicy)
            : CompareRowsByContentHash(sourceTable, targetTable, retentionPolicy);
        var sourceRowCount = sourceTable?.RowCount;
        var targetRowCount = targetTable?.RowCount;

        return new DataDifferenceModel
        {
            SchemaName = requestedTable.SchemaName,
            TableName = requestedTable.TableName,
            KindCode = ResolveTableKind(sourceRowCount, targetRowCount),
            SourceRowCount = sourceRowCount,
            TargetRowCount = targetRowCount,
            RowCountDifference = sourceRowCount.HasValue && targetRowCount.HasValue
                ? sourceRowCount.Value - targetRowCount.Value
                : null,
            SourceHash = sourceHash,
            TargetHash = targetHash,
            HasRowLevelDifference = rowDifferences.Count > 0,
            RowDifferences = rowDifferences
        };
    }

    // islevi: Iki tarafta da ayni adlarla bulunan composite PK kolonlarini eslestirme anahtari olarak secer.
    private static List<string> ResolveComparablePrimaryKey(
        TableDataSnapshotModel? sourceTable,
        TableDataSnapshotModel? targetTable)
    {
        var candidate = sourceTable?.PrimaryKeyColumns.Count > 0
            ? sourceTable.PrimaryKeyColumns
            : targetTable?.PrimaryKeyColumns ?? new List<string>();
        if (candidate.Count == 0)
        {
            return new List<string>();
        }

        var sourceColumns = sourceTable?.ColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targetColumns = targetTable?.ColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existsInSource = sourceColumns is null || candidate.All(sourceColumns.Contains);
        var existsInTarget = targetColumns is null || candidate.All(targetColumns.Contains);
        return existsInSource && existsInTarget ? candidate.ToList() : new List<string>();
    }

    // islevi: PK degeri ayni satirlari eslestirir; eklenen/silinen satirlari ve Modified satirin degisen hucrelerini uretir.
    private static List<DataRowDifferenceModel> CompareRowsByPrimaryKey(
        TableDataSnapshotModel? sourceTable,
        TableDataSnapshotModel? targetTable,
        List<string> primaryKeyColumns,
        ValueRetentionPolicy retentionPolicy)
    {
        // Secili PK kolonlari bir tarafta benzersiz degilse (kaynak/hedef PK yapisi farkli), duplicate
        // satirlari sozlukte ezmek yerine multiset content-hash karsilastirmasina duser - false-negative olmaz.
        if (!TryBuildRowsByPrimaryKey(sourceTable, primaryKeyColumns, out var sourceRows) ||
            !TryBuildRowsByPrimaryKey(targetTable, primaryKeyColumns, out var targetRows))
        {
            return CompareRowsByContentHash(sourceTable, targetTable, retentionPolicy);
        }

        var rowKeys = sourceRows.Keys
            .Union(targetRows.Keys, StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal);
        var differences = new List<DataRowDifferenceModel>();

        foreach (var rowKey in rowKeys)
        {
            var hasSource = sourceRows.TryGetValue(rowKey, out var sourceRow);
            var hasTarget = targetRows.TryGetValue(rowKey, out var targetRow);
            if (!hasSource || !hasTarget)
            {
                differences.Add(new DataRowDifferenceModel
                {
                    PrimaryKeyValue = RedactPrimaryKey(rowKey, retentionPolicy),
                    KindCode = hasSource
                        ? DifferenceKindCodes.OnlyInSource
                        : DifferenceKindCodes.OnlyInTarget
                });
                continue;
            }

            var valueDifferences = CompareValues(sourceRow!, targetRow!, primaryKeyColumns, retentionPolicy);
            if (valueDifferences.Count > 0)
            {
                differences.Add(new DataRowDifferenceModel
                {
                    PrimaryKeyValue = RedactPrimaryKey(rowKey, retentionPolicy),
                    KindCode = DifferenceKindCodes.Modified,
                    ValueDifferences = valueDifferences
                });
            }
        }

        return differences;
    }

    // islevi: PK olmayan tablolari tum-row canonical hash + occurrence ile multiset olarak exact karsilastirir.
    // sistemdeki gorevi: Stable kimlik olmadigi icin Modified cell yorumu uydurmaz; yalniz gercek OnlyInSource/OnlyInTarget satirlarini raporlar.
    private static List<DataRowDifferenceModel> CompareRowsByContentHash(
        TableDataSnapshotModel? sourceTable,
        TableDataSnapshotModel? targetTable,
        ValueRetentionPolicy retentionPolicy)
    {
        var sourceRows = BuildRowsByContentHash(sourceTable);
        var targetRows = BuildRowsByContentHash(targetTable);
        var differences = new List<DataRowDifferenceModel>();

        foreach (var rowKey in sourceRows.Keys
                     .Union(targetRows.Keys, StringComparer.Ordinal)
                     .OrderBy(key => key, StringComparer.Ordinal))
        {
            var inSource = sourceRows.ContainsKey(rowKey);
            var inTarget = targetRows.ContainsKey(rowKey);
            if (inSource == inTarget)
            {
                continue;
            }

            differences.Add(new DataRowDifferenceModel
            {
                PrimaryKeyValue = RedactPrimaryKey(rowKey, retentionPolicy),
                KindCode = inSource
                    ? DifferenceKindCodes.OnlyInSource
                    : DifferenceKindCodes.OnlyInTarget
            });
        }

        return differences;
    }

    // islevi: Modified PK satirinin kolon union'unda presence/null/value farklarini hucre listesine cevirir.
    private static List<DataValueDifferenceModel> CompareValues(
        TableDataRowModel sourceRow,
        TableDataRowModel targetRow,
        List<string> primaryKeyColumns,
        ValueRetentionPolicy retentionPolicy)
    {
        var primaryKeySet = primaryKeyColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var columnNames = sourceRow.Values.Keys
            .Union(targetRow.Values.Keys, StringComparer.OrdinalIgnoreCase)
            .Where(columnName => !primaryKeySet.Contains(columnName))
            .OrderBy(columnName => columnName, StringComparer.OrdinalIgnoreCase);
        var differences = new List<DataValueDifferenceModel>();

        foreach (var columnName in columnNames)
        {
            var hasSource = sourceRow.Values.TryGetValue(columnName, out var sourceValue);
            var hasTarget = targetRow.Values.TryGetValue(columnName, out var targetValue);
            if (hasSource == hasTarget && string.Equals(sourceValue, targetValue, StringComparison.Ordinal))
            {
                continue;
            }

            differences.Add(new DataValueDifferenceModel
            {
                ColumnName = columnName,
                SourceValue = ValueRedactor.Redact(sourceValue, retentionPolicy),
                TargetValue = ValueRedactor.Redact(targetValue, retentionPolicy)
            });
        }

        return differences;
    }

    // islevi: Table snapshot satirlarini composite PK canonical metniyle indexler; PK benzersiz degilse false doner.
    // sistemdeki gorevi: TryAdd duplicate PK'yi yakalar (ezme yok); benzersizlik bozulursa cagiran content-hash yoluna duser.
    private static bool TryBuildRowsByPrimaryKey(
        TableDataSnapshotModel? table,
        List<string> primaryKeyColumns,
        out Dictionary<string, TableDataRowModel> rows)
    {
        rows = new Dictionary<string, TableDataRowModel>(StringComparer.Ordinal);
        if (table is null)
        {
            return true;
        }

        foreach (var row in table.Rows)
        {
            if (!rows.TryAdd(BuildPrimaryKey(row, primaryKeyColumns), row))
            {
                return false;
            }
        }

        return true;
    }

    // islevi: PK olmayan satirlari canonical whole-row hash ve tekrar sirasi ile kayipsiz indexler.
    private static Dictionary<string, TableDataRowModel> BuildRowsByContentHash(TableDataSnapshotModel? table)
    {
        var rows = new Dictionary<string, TableDataRowModel>(StringComparer.Ordinal);
        if (table is null)
        {
            return rows;
        }

        var occurrenceByHash = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var row in table.Rows.OrderBy(BuildCanonicalRow, StringComparer.Ordinal))
        {
            var hash = ComputeHash(BuildCanonicalRow(row));
            var occurrence = occurrenceByHash.GetValueOrDefault(hash) + 1;
            occurrenceByHash[hash] = occurrence;
            rows[$"{hash}{ComparisonCanonicalTextConstants.OccurrenceSeparator}{occurrence}"] = row;
        }

        return rows;
    }

    // islevi: Composite PK degerlerini kolon sirasini koruyarak ve delimiter karakterlerini escape ederek tek metne cevirir.
    private static string BuildPrimaryKey(TableDataRowModel row, List<string> primaryKeyColumns)
        => string.Join(ComparisonCanonicalTextConstants.KeySeparator, primaryKeyColumns.Select(columnName =>
            EscapeKeyPart(row.Values.GetValueOrDefault(columnName))));

    // islevi: PK eslestirmesi bittikten sonra ham anahtar metnini politika disina cikmadan bulgu temsiline cevirir.
    private static string RedactPrimaryKey(string primaryKeyValue, ValueRetentionPolicy retentionPolicy)
        => ValueRedactor.Redact(primaryKeyValue, retentionPolicy) ?? string.Empty;

    // islevi: Tum tablo satirlarini sirali canonical metin uzerinden SHA-256 parmak izine cevirir.
    private static string? ComputeTableHash(TableDataSnapshotModel? table)
    {
        if (table is null)
        {
            return null;
        }

        var canonicalTable = string.Join(
            ComparisonCanonicalTextConstants.RowSeparator,
            table.Rows.Select(BuildCanonicalRow).OrderBy(row => row, StringComparer.Ordinal));
        return ComputeHash(canonicalTable);
    }

    // islevi: Kolon adlarini ve hucre degerlerini kararli sirada yazarak collision-safe row metni uretir.
    private static string BuildCanonicalRow(TableDataRowModel row)
        => string.Join(
            ComparisonCanonicalTextConstants.FieldSeparator,
            row.Values
                .OrderBy(value => value.Key, StringComparer.OrdinalIgnoreCase)
                .Select(value =>
                {
                    var normalizedName = value.Key.ToUpperInvariant();
                    return $"{normalizedName.Length}{ComparisonCanonicalTextConstants.LengthSeparator}" +
                           $"{normalizedName}{ComparisonCanonicalTextConstants.DefinitionKeyValueSeparator}" +
                           EncodeValue(value.Value);
                }));

    // islevi: Hucre degerini SQL NULL'u gercek "<NULL>" metninden ayiran, uzunluk-etiketli parmak izine cevirir.
    // sistemdeki gorevi: NULL tek basina "N"; dolu deger "V{uzunluk}:{deger}" olur - dolu deger daima 'V' ile basladigindan hicbir gercek metin NULL sentineliyle ayni hash'e dusemez.
    private static string EncodeValue(string? value)
        => value is null
            ? ComparisonCanonicalTextConstants.NullValueMarker
            : $"{ComparisonCanonicalTextConstants.ValueMarker}{value.Length}" +
              $"{ComparisonCanonicalTextConstants.LengthSeparator}{value}";

    // islevi: Canonical metnin SHA-256 hex temsilini uretir.
    private static string ComputeHash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    // islevi: Composite PK metnindeki escape ve ayirac karakterlerini geri-donulebilir bicimde korur.
    private static string EscapeKeyPart(string? value)
        => (value ?? ComparisonCanonicalTextConstants.NullKeyPlaceholder)
            .Replace(
                ComparisonCanonicalTextConstants.EscapeCharacter,
                ComparisonCanonicalTextConstants.EscapedEscapeCharacter,
                StringComparison.Ordinal)
            .Replace(
                ComparisonCanonicalTextConstants.KeySeparator,
                ComparisonCanonicalTextConstants.EscapedKeySeparator,
                StringComparison.Ordinal);

    // islevi: Tablo-level fark yonunu varlik ve signed row-count farkindan cozer.
    private static string ResolveTableKind(long? sourceCount, long? targetCount)
    {
        if (!sourceCount.HasValue)
        {
            return DifferenceKindCodes.OnlyInTarget;
        }

        if (!targetCount.HasValue)
        {
            return DifferenceKindCodes.OnlyInSource;
        }

        if (sourceCount.Value > targetCount.Value)
        {
            return DifferenceKindCodes.OnlyInSource;
        }

        return sourceCount.Value < targetCount.Value
            ? DifferenceKindCodes.OnlyInTarget
            : DifferenceKindCodes.Modified;
    }

    // islevi: Table data snapshot listesini sema+tablo anahtariyla indexler.
    private static Dictionary<string, TableDataSnapshotModel> BuildTableByKey(List<TableDataSnapshotModel> tables)
        => tables.ToDictionary(
            table => BuildTableKey(table.SchemaName, table.TableName),
            StringComparer.OrdinalIgnoreCase);

    // islevi: Sema+tablo adresini dictionary anahtarina cevirir.
    private static string BuildTableKey(string schemaName, string tableName)
        => string.Join(ComparisonCanonicalTextConstants.KeySeparator, schemaName.Trim(), tableName.Trim());
}
