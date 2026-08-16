using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Ayni turdeki base ve target spec nesne koleksiyonlarini kararli kimlikle karsilastirir.
// sistemdeki gorevi: Operasyon ve sonraki sema diff adimlarinin OnlyInSource, OnlyInTarget ve Modified esleme mantigini tek kez kurar.
public class SpecCollectionComparer : ITransientDependency
{
    // Koleksiyonlari anahtarla indeksleyip varlik ve tanim degisikliklerini siddetlendirilmemis farklara cevirir.
    public List<SpecDifferenceModel> Compare<TItem>(
        IEnumerable<TItem> sourceItems,
        IEnumerable<TItem> targetItems,
        Func<TItem, string> keySelector,
        Func<TItem, FindingAddress> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, string?> valueSelector,
        Func<TItem, TItem, (string? OldValue, string? NewValue)> changeSummarySelector,
        string? onlyInSourceKindCode,
        string? onlyInTargetKindCode,
        string? modifiedKindCode,
        string directionCode)
    {
        var sourceByKey = BuildDictionary(sourceItems, keySelector);
        var targetByKey = BuildDictionary(targetItems, keySelector);
        var differences = new List<SpecDifferenceModel>();

        AddOnlyInSource(
            sourceByKey,
            targetByKey,
            addressSelector,
            valueSelector,
            onlyInSourceKindCode,
            directionCode,
            differences);
        AddOnlyInTarget(
            sourceByKey,
            targetByKey,
            addressSelector,
            valueSelector,
            onlyInTargetKindCode,
            directionCode,
            differences);
        AddModified(
            sourceByKey,
            targetByKey,
            addressSelector,
            definitionSelector,
            changeSummarySelector,
            modifiedKindCode,
            directionCode,
            differences);

        return differences;
    }

    // Iki koleksiyonda ayni kararli anahtari tasiyan nesneleri anahtar sirasiyla dondurur.
    public IEnumerable<(TItem Source, TItem Target)> EnumerateMatched<TItem>(
        IEnumerable<TItem> sourceItems,
        IEnumerable<TItem> targetItems,
        Func<TItem, string> keySelector)
    {
        var sourceByKey = BuildDictionary(sourceItems, keySelector);
        var targetByKey = BuildDictionary(targetItems, keySelector);

        foreach (var (key, sourceItem) in sourceByKey.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (targetByKey.TryGetValue(key, out var targetItem))
            {
                yield return (sourceItem, targetItem);
            }
        }
    }

    // Once kararli kimligi, kalan nesneleri ise ozellik kumesi benzerligini kullanarak deterministik bire-bir esler.
    public (List<(TItem Source, TItem Target)> Pairs, List<TItem> OnlyInSource, List<TItem> OnlyInTarget)
        MatchWithSimilarityFallback<TItem>(
            IEnumerable<TItem> sourceItems,
            IEnumerable<TItem> targetItems,
            Func<TItem, string> keySelector,
            Func<TItem, IEnumerable<string>> featureSelector,
            double similarityThreshold)
    {
        var sourceByKey = BuildDictionary(sourceItems, keySelector);
        var targetByKey = BuildDictionary(targetItems, keySelector);
        var pairs = EnumerateMatched(sourceByKey.Values, targetByKey.Values, keySelector).ToList();
        var onlyInSource = sourceByKey.Values.Where(item => !targetByKey.ContainsKey(keySelector(item))).ToList();
        var onlyInTarget = targetByKey.Values.Where(item => !sourceByKey.ContainsKey(keySelector(item))).ToList();
        var targetFeatures = onlyInTarget.ToDictionary(
            keySelector,
            item => featureSelector(item).ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);
        var targetsByFeature = onlyInTarget
            .SelectMany(item => targetFeatures[keySelector(item)].Select(feature => (feature, item)))
            .GroupBy(entry => entry.feature, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.item), StringComparer.Ordinal);
        var candidates = new List<(TItem Source, TItem Target, double Score)>();

        foreach (var source in onlyInSource)
        {
            var sourceFeatures = featureSelector(source).ToHashSet(StringComparer.Ordinal);
            var overlaps = sourceFeatures
                .Where(targetsByFeature.ContainsKey)
                .SelectMany(feature => targetsByFeature[feature])
                .GroupBy(keySelector, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
            candidates.AddRange(overlaps
                .Select(overlap => (
                    Source: source,
                    Target: targetByKey[overlap.Key],
                    Score: (double)overlap.Value /
                           (sourceFeatures.Count + targetFeatures[overlap.Key].Count - overlap.Value)))
                .Where(candidate => candidate.Score >= similarityThreshold));
        }

        var matchedSourceKeys = new HashSet<string>(StringComparer.Ordinal);
        var matchedTargetKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in candidates
                     .OrderByDescending(candidate => candidate.Score)
                     .ThenBy(candidate => keySelector(candidate.Source), StringComparer.Ordinal)
                     .ThenBy(candidate => keySelector(candidate.Target), StringComparer.Ordinal))
        {
            var sourceKey = keySelector(candidate.Source);
            var targetKey = keySelector(candidate.Target);
            if (matchedSourceKeys.Contains(sourceKey) || matchedTargetKeys.Contains(targetKey))
            {
                continue;
            }

            matchedSourceKeys.Add(sourceKey);
            matchedTargetKeys.Add(targetKey);
            pairs.Add((candidate.Source, candidate.Target));
        }

        return (
            pairs,
            onlyInSource.Where(item => !matchedSourceKeys.Contains(keySelector(item))).ToList(),
            onlyInTarget.Where(item => !matchedTargetKeys.Contains(keySelector(item))).ToList());
    }

    // Bulgulari tum adres, tur, yon ve deger alanlariyla tek kararli siraya dizer.
    public List<SpecDifferenceModel> SortDifferences(IEnumerable<SpecDifferenceModel> differences)
        => differences
            .OrderBy(difference => difference.Address.OperationId, StringComparer.Ordinal)
            .ThenBy(difference => difference.Address.HttpMethod, StringComparer.Ordinal)
            .ThenBy(difference => difference.Address.Path, StringComparer.Ordinal)
            .ThenBy(difference => difference.Address.SchemaName, StringComparer.Ordinal)
            .ThenBy(difference => difference.Address.PropertyPath, StringComparer.Ordinal)
            .ThenBy(difference => difference.Address.ParameterName, StringComparer.Ordinal)
            .ThenBy(difference => difference.Address.ResponseStatus, StringComparer.Ordinal)
            .ThenBy(difference => difference.Address.MediaType, StringComparer.Ordinal)
            .ThenBy(difference => difference.KindCode, StringComparer.Ordinal)
            .ThenBy(difference => difference.DirectionCode, StringComparer.Ordinal)
            .ThenBy(difference => difference.OldValue, StringComparer.Ordinal)
            .ThenBy(difference => difference.NewValue, StringComparer.Ordinal)
            .ToList();

    // Duplicate kimlik halinde ilk normalize kaydi koruyarak kararli anahtar sozlugu kurar.
    private static Dictionary<string, TItem> BuildDictionary<TItem>(
        IEnumerable<TItem> items,
        Func<TItem, string> keySelector)
        => items
            .GroupBy(keySelector, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

    // Hedefte bulunmayan kaynak nesnelerini kararli anahtar sirasiyla fark listesine ekler.
    private static void AddOnlyInSource<TItem>(
        IReadOnlyDictionary<string, TItem> sourceByKey,
        IReadOnlyDictionary<string, TItem> targetByKey,
        Func<TItem, FindingAddress> addressSelector,
        Func<TItem, string?> valueSelector,
        string? kindCode,
        string directionCode,
        ICollection<SpecDifferenceModel> differences)
    {
        if (kindCode is null)
        {
            return;
        }

        foreach (var (key, sourceItem) in sourceByKey.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!targetByKey.ContainsKey(key))
            {
                differences.Add(SpecDifferenceFactory.OnlyInSource(
                    kindCode,
                    directionCode,
                    addressSelector(sourceItem),
                    valueSelector(sourceItem)));
            }
        }
    }

    // Kaynakta bulunmayan hedef nesnelerini kararli anahtar sirasiyla fark listesine ekler.
    private static void AddOnlyInTarget<TItem>(
        IReadOnlyDictionary<string, TItem> sourceByKey,
        IReadOnlyDictionary<string, TItem> targetByKey,
        Func<TItem, FindingAddress> addressSelector,
        Func<TItem, string?> valueSelector,
        string? kindCode,
        string directionCode,
        ICollection<SpecDifferenceModel> differences)
    {
        if (kindCode is null)
        {
            return;
        }

        foreach (var (key, targetItem) in targetByKey.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!sourceByKey.ContainsKey(key))
            {
                differences.Add(SpecDifferenceFactory.OnlyInTarget(
                    kindCode,
                    directionCode,
                    addressSelector(targetItem),
                    valueSelector(targetItem)));
            }
        }
    }

    // Ortak kimlikli fakat farkli tanimli nesneleri degisim ozetiyle fark listesine ekler.
    private static void AddModified<TItem>(
        IReadOnlyDictionary<string, TItem> sourceByKey,
        IReadOnlyDictionary<string, TItem> targetByKey,
        Func<TItem, FindingAddress> addressSelector,
        Func<TItem, string?> definitionSelector,
        Func<TItem, TItem, (string? OldValue, string? NewValue)> changeSummarySelector,
        string? kindCode,
        string directionCode,
        ICollection<SpecDifferenceModel> differences)
    {
        if (kindCode is null)
        {
            return;
        }

        foreach (var (key, sourceItem) in sourceByKey.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!targetByKey.TryGetValue(key, out var targetItem) ||
                string.Equals(definitionSelector(sourceItem), definitionSelector(targetItem), StringComparison.Ordinal))
            {
                continue;
            }

            var change = changeSummarySelector(sourceItem, targetItem);
            differences.Add(SpecDifferenceFactory.Modified(
                kindCode,
                directionCode,
                addressSelector(sourceItem),
                change.OldValue,
                change.NewValue));
        }
    }
}
