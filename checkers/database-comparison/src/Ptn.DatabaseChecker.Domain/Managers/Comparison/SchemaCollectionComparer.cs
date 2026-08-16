using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Ayni turdeki source/target nesne koleksiyonlarini generic olarak karsilastirir.
// sistemdeki gorevi: Tablo, kolon, index, constraint, trigger ve schema-level nesnelerde OnlyInSource/OnlyInTarget/Modified yon mantigi tek kez yazilir; tur ozel key/tanim/adres hook'lari manager'dan gelir.
public class SchemaCollectionComparer : ITransientDependency
{
    // Raw tanim metinlerini kiyas oncesi ortak forma indiren servis.
    private readonly SchemaDefinitionNormalizer _normalizer;

    public SchemaCollectionComparer(SchemaDefinitionNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    // islevi: Verilen koleksiyonlardan kararli yon bilgisi tasiyan SchemaDifferenceModel listesi uretir.
    public List<SchemaDifferenceModel> Compare<TItem>(
        IEnumerable<TItem> sourceItems,
        IEnumerable<TItem> targetItems,
        Func<TItem, string> keySelector,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, TItem, string?> changeSummarySelector,
        Func<TItem?, TItem?, string> confidenceSelector)
    {
        var sourceByKey = BuildDictionary(sourceItems, keySelector);
        var targetByKey = BuildDictionary(targetItems, keySelector);
        var differences = new List<SchemaDifferenceModel>();

        AddMissingFromTarget(sourceByKey, targetByKey, addressSelector, definitionSelector, confidenceSelector, differences);
        AddMissingFromSource(sourceByKey, targetByKey, addressSelector, definitionSelector, confidenceSelector, differences);
        AddModified(sourceByKey, targetByKey, addressSelector, definitionSelector, changeSummarySelector, confidenceSelector, differences);

        return differences;
    }

    // islevi: 0.1.x sabit guven kodlu karsilastirma imzasini yeni secici akisa yonlendirir.
    public List<SchemaDifferenceModel> Compare<TItem>(
        IEnumerable<TItem> sourceItems,
        IEnumerable<TItem> targetItems,
        Func<TItem, string> keySelector,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, TItem, string?> changeSummarySelector,
        string confidenceCode)
        => Compare(
            sourceItems,
            targetItems,
            keySelector,
            addressSelector,
            definitionSelector,
            changeSummarySelector,
            (_, _) => confidenceCode);

    // islevi: Once kararli nesne anahtariyla, eslesmeyenleri ise yalnizca tekil yapisal anahtarla eslestirerek rename kaynakli yalanci eksik/fazla bulgularini engeller.
    // sistemdeki gorevi: Index gibi adi semantik kimlik olmayan nesnelerde ayni yapinin farkli adlarini guvenle esler; ayni yapidan birden fazla varsa belirsiz eslestirme yapmaz.
    public List<SchemaDifferenceModel> CompareWithUniqueFallback<TItem>(
        IEnumerable<TItem> sourceItems,
        IEnumerable<TItem> targetItems,
        Func<TItem, string> primaryKeySelector,
        Func<TItem, string> fallbackKeySelector,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, TItem, string?> changeSummarySelector,
        Func<TItem?, TItem?, string> confidenceSelector)
    {
        var sourceByPrimaryKey = BuildDictionary(sourceItems, primaryKeySelector);
        var targetByPrimaryKey = BuildDictionary(targetItems, primaryKeySelector);
        var matchedPairs = BuildPrimaryMatches(sourceByPrimaryKey, targetByPrimaryKey);
        var matchedSourceKeys = matchedPairs
            .Select(pair => primaryKeySelector(pair.Source))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var matchedTargetKeys = matchedPairs
            .Select(pair => primaryKeySelector(pair.Target))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AddUniqueFallbackMatches(
            sourceByPrimaryKey,
            targetByPrimaryKey,
            fallbackKeySelector,
            primaryKeySelector,
            matchedSourceKeys,
            matchedTargetKeys,
            matchedPairs);

        var differences = new List<SchemaDifferenceModel>();
        AddUnmatchedSourceItems(
            sourceByPrimaryKey,
            matchedSourceKeys,
            addressSelector,
            definitionSelector,
            confidenceSelector,
            differences);
        AddUnmatchedTargetItems(
            targetByPrimaryKey,
            matchedTargetKeys,
            addressSelector,
            definitionSelector,
            confidenceSelector,
            differences);
        AddModifiedPairs(
            matchedPairs,
            addressSelector,
            definitionSelector,
            changeSummarySelector,
            confidenceSelector,
            differences);
        return differences;
    }

    // islevi: 0.1.x sabit guven kodlu fallback imzasini yeni secici akisa yonlendirir.
    public List<SchemaDifferenceModel> CompareWithUniqueFallback<TItem>(
        IEnumerable<TItem> sourceItems,
        IEnumerable<TItem> targetItems,
        Func<TItem, string> primaryKeySelector,
        Func<TItem, string> fallbackKeySelector,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, TItem, string?> changeSummarySelector,
        string confidenceCode)
        => CompareWithUniqueFallback(
            sourceItems,
            targetItems,
            primaryKeySelector,
            fallbackKeySelector,
            addressSelector,
            definitionSelector,
            changeSummarySelector,
            (_, _) => confidenceCode);

    // islevi: Ayni anahtara sahip source/target ciftlerini kararli sirada dondurur.
    // sistemdeki gorevi: Ortak tablonun kolon/index/constraint/trigger alt farklari icin manager ayni anahtar sozlugunu tekrar kurmadan eslesmis tablolari bu tek noktadan gezer; keyed-index mantigi comparer'da tek kalir.
    public IEnumerable<(TItem Source, TItem Target)> EnumerateMatched<TItem>(
        IEnumerable<TItem> sourceItems,
        IEnumerable<TItem> targetItems,
        Func<TItem, string> keySelector)
    {
        var sourceByKey = BuildDictionary(sourceItems, keySelector);
        var targetByKey = BuildDictionary(targetItems, keySelector);

        foreach (var (key, sourceItem) in sourceByKey.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (targetByKey.TryGetValue(key, out var targetItem))
            {
                yield return (sourceItem, targetItem);
            }
        }
    }

    // islevi: Duplicate katalog satirlari halinde ilk kaydi koruyup karsilastirma anahtar sozlugu kurar.
    private static Dictionary<string, TItem> BuildDictionary<TItem>(
        IEnumerable<TItem> items,
        Func<TItem, string> keySelector)
    {
        return items
            .GroupBy(keySelector, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    // islevi: Ayni birincil anahtara sahip source/target nesnelerini ilk eslesme listesine alir.
    private static List<(TItem Source, TItem Target)> BuildPrimaryMatches<TItem>(
        Dictionary<string, TItem> sourceByPrimaryKey,
        Dictionary<string, TItem> targetByPrimaryKey)
    {
        return sourceByPrimaryKey
            .Where(pair => targetByPrimaryKey.ContainsKey(pair.Key))
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => (pair.Value, targetByPrimaryKey[pair.Key]))
            .ToList();
    }

    // islevi: Adla eslesmeyen nesneleri yapisal anahtar her iki tarafta da tekilse eslestirir.
    // sistemdeki gorevi: Rename'i tolere ederken duplicate yapilarda keyfi pairing yapilip gercek farklarin saklanmasini engeller.
    private static void AddUniqueFallbackMatches<TItem>(
        Dictionary<string, TItem> sourceByPrimaryKey,
        Dictionary<string, TItem> targetByPrimaryKey,
        Func<TItem, string> fallbackKeySelector,
        Func<TItem, string> primaryKeySelector,
        HashSet<string> matchedSourceKeys,
        HashSet<string> matchedTargetKeys,
        List<(TItem Source, TItem Target)> matchedPairs)
    {
        var sourceGroups = sourceByPrimaryKey.Values
            .Where(item => !matchedSourceKeys.Contains(primaryKeySelector(item)))
            .GroupBy(fallbackKeySelector, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var targetGroups = targetByPrimaryKey.Values
            .Where(item => !matchedTargetKeys.Contains(primaryKeySelector(item)))
            .GroupBy(fallbackKeySelector, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        foreach (var (fallbackKey, sourceGroup) in sourceGroups.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (sourceGroup.Count != 1 ||
                !targetGroups.TryGetValue(fallbackKey, out var targetGroup) ||
                targetGroup.Count != 1)
            {
                continue;
            }

            var sourceItem = sourceGroup[0];
            var targetItem = targetGroup[0];
            matchedPairs.Add((sourceItem, targetItem));
            matchedSourceKeys.Add(primaryKeySelector(sourceItem));
            matchedTargetKeys.Add(primaryKeySelector(targetItem));
        }
    }

    // islevi: Her iki eslestirme asamasinda da karsilik bulamayan kaynak nesnelerini OnlyInSource bulgusuna cevirir.
    private static void AddUnmatchedSourceItems<TItem>(
        Dictionary<string, TItem> sourceByPrimaryKey,
        HashSet<string> matchedSourceKeys,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem?, TItem?, string> confidenceSelector,
        List<SchemaDifferenceModel> differences)
    {
        foreach (var (key, sourceItem) in sourceByPrimaryKey.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (!matchedSourceKeys.Contains(key))
            {
                differences.Add(SchemaDifferenceFactory.OnlyInSource(
                    addressSelector(sourceItem),
                    definitionSelector(sourceItem),
                    confidenceSelector(sourceItem, default)));
            }
        }
    }

    // islevi: Her iki eslestirme asamasinda da karsilik bulamayan hedef nesnelerini OnlyInTarget bulgusuna cevirir.
    private static void AddUnmatchedTargetItems<TItem>(
        Dictionary<string, TItem> targetByPrimaryKey,
        HashSet<string> matchedTargetKeys,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem?, TItem?, string> confidenceSelector,
        List<SchemaDifferenceModel> differences)
    {
        foreach (var (key, targetItem) in targetByPrimaryKey.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (!matchedTargetKeys.Contains(key))
            {
                differences.Add(SchemaDifferenceFactory.OnlyInTarget(
                    addressSelector(targetItem),
                    definitionSelector(targetItem),
                    confidenceSelector(default, targetItem)));
            }
        }
    }

    // islevi: Ad veya tekil yapisal anahtarla eslesen ciftlerde gercek tanim degisikliklerini Modified bulgusuna cevirir.
    private void AddModifiedPairs<TItem>(
        List<(TItem Source, TItem Target)> matchedPairs,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, TItem, string?> changeSummarySelector,
        Func<TItem?, TItem?, string> confidenceSelector,
        List<SchemaDifferenceModel> differences)
    {
        foreach (var (sourceItem, targetItem) in matchedPairs)
        {
            var sourceDefinition = definitionSelector(sourceItem);
            var targetDefinition = definitionSelector(targetItem);
            var confidenceCode = confidenceSelector(sourceItem, targetItem);
            if (DefinitionsEqual(sourceDefinition, targetDefinition) &&
                !string.Equals(
                    confidenceCode,
                    ComparisonConfidenceCodes.Incomparable,
                    StringComparison.Ordinal))
            {
                continue;
            }

            differences.Add(SchemaDifferenceFactory.Modified(
                addressSelector(sourceItem),
                sourceDefinition,
                targetDefinition,
                changeSummarySelector(sourceItem, targetItem),
                confidenceCode));
        }
    }

    // islevi: Kaynakta olup hedefte olmayan kayitlari fark listesine ekler.
    private static void AddMissingFromTarget<TItem>(
        Dictionary<string, TItem> sourceByKey,
        Dictionary<string, TItem> targetByKey,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem?, TItem?, string> confidenceSelector,
        List<SchemaDifferenceModel> differences)
    {
        foreach (var (key, sourceItem) in sourceByKey)
        {
            if (!targetByKey.ContainsKey(key))
            {
                differences.Add(SchemaDifferenceFactory.OnlyInSource(
                    addressSelector(sourceItem),
                    definitionSelector(sourceItem),
                    confidenceSelector(sourceItem, default)));
            }
        }
    }

    // islevi: Hedefte olup kaynakta olmayan kayitlari fark listesine ekler.
    private static void AddMissingFromSource<TItem>(
        Dictionary<string, TItem> sourceByKey,
        Dictionary<string, TItem> targetByKey,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem?, TItem?, string> confidenceSelector,
        List<SchemaDifferenceModel> differences)
    {
        foreach (var (key, targetItem) in targetByKey)
        {
            if (!sourceByKey.ContainsKey(key))
            {
                differences.Add(SchemaDifferenceFactory.OnlyInTarget(
                    addressSelector(targetItem),
                    definitionSelector(targetItem),
                    confidenceSelector(default, targetItem)));
            }
        }
    }

    // islevi: Iki tarafta da olan ama normalize tanimi farkli kayitlari Modified bulgusuna cevirir.
    private void AddModified<TItem>(
        Dictionary<string, TItem> sourceByKey,
        Dictionary<string, TItem> targetByKey,
        Func<TItem, SchemaComparisonAddressModel> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, TItem, string?> changeSummarySelector,
        Func<TItem?, TItem?, string> confidenceSelector,
        List<SchemaDifferenceModel> differences)
    {
        foreach (var (key, sourceItem) in sourceByKey)
        {
            if (!targetByKey.TryGetValue(key, out var targetItem))
            {
                continue;
            }

            var sourceDefinition = definitionSelector(sourceItem);
            var targetDefinition = definitionSelector(targetItem);
            var confidenceCode = confidenceSelector(sourceItem, targetItem);
            if (DefinitionsEqual(sourceDefinition, targetDefinition) &&
                !string.Equals(
                    confidenceCode,
                    ComparisonConfidenceCodes.Incomparable,
                    StringComparison.Ordinal))
            {
                continue;
            }

            differences.Add(SchemaDifferenceFactory.Modified(
                addressSelector(sourceItem),
                sourceDefinition,
                targetDefinition,
                changeSummarySelector(sourceItem, targetItem),
                confidenceCode));
        }
    }

    // islevi: Raw tanim metinlerini normalize edip anlamsal olmayan bosluk/parantez farklarini eler.
    private bool DefinitionsEqual(string? sourceDefinition, string? targetDefinition)
    {
        return string.Equals(
            _normalizer.NormalizeDefinition(sourceDefinition),
            _normalizer.NormalizeDefinition(targetDefinition),
            StringComparison.Ordinal);
    }
}
