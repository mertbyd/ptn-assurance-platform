using Ptn.ApiContractChecker.Constants.Differences;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.Domain.Services;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Iki normalize snapshot'in operasyonlardan erisilebilen sema ve ozellik sozlesmelerini karsilastirir.
// sistemdeki gorevi: KBP-612 yon-duyarli DTO farklarini ve yapisal yeniden adlandirmalari I/O ve siddet siniflandirmasi olmadan uretir.
public class SpecSchemaComparisonManager : DomainService
{
    // Kimlik, rename fallback ve deterministik siralama davranisini tek merkezden saglayan comparer.
    private readonly SpecCollectionComparer _collectionComparer;

    // Sema manager'ini mevcut generic koleksiyon esleyicisiyle kurar.
    public SpecSchemaComparisonManager(SpecCollectionComparer collectionComparer)
    {
        _collectionComparer = collectionComparer;
    }

    // Sema esleme, yon ve property kurallarini tek okunabilir comparison akisinda birlestirir.
    public List<SpecDifferenceModel> Compare(
        SpecSnapshotModel baseSnapshot,
        SpecSnapshotModel targetSnapshot)
    {
        var baseUsage = ResolveUsage(baseSnapshot);
        var targetUsage = ResolveUsage(targetSnapshot);
        var schemas = MatchSchemas(
            baseSnapshot.Schemas,
            targetSnapshot.Schemas,
            baseUsage.Keys.Concat(targetUsage.Keys).ToHashSet(StringComparer.Ordinal));
        var differences = CompareSchemaPresence(schemas, baseUsage, targetUsage);
        differences.AddRange(CompareSchemaRenames(schemas.Pairs, baseUsage, targetUsage));
        differences.AddRange(CompareMatchedProperties(schemas.Pairs, baseUsage, targetUsage));
        return _collectionComparer.SortDifferences(differences);
    }

    // Sema koleksiyonunu once adla, kalanlari property ortusmesiyle esler.
    private (List<(SpecSchemaModel Source, SpecSchemaModel Target)> Pairs,
        List<SpecSchemaModel> OnlyInSource,
        List<SpecSchemaModel> OnlyInTarget) MatchSchemas(
        IEnumerable<SpecSchemaModel> sourceSchemas,
        IEnumerable<SpecSchemaModel> targetSchemas,
        IReadOnlySet<string> reachableSchemaNames)
        => _collectionComparer.MatchWithSimilarityFallback(
            sourceSchemas,
            targetSchemas,
            schema => schema.Name,
            schema => reachableSchemaNames.Contains(schema.Name)
                ? schema.Properties.Select(property => property.Name)
                : Enumerable.Empty<string>(),
            SpecSchemaComparisonConstants.RenameSimilarityThreshold);

    // Rename ile eslesmeyen semalarin ekleme ve silme bulgularini kullanim yonlerine dagitir.
    private static List<SpecDifferenceModel> CompareSchemaPresence(
        (List<(SpecSchemaModel Source, SpecSchemaModel Target)> Pairs,
            List<SpecSchemaModel> OnlyInSource,
            List<SpecSchemaModel> OnlyInTarget) schemas,
        IReadOnlyDictionary<string, HashSet<string>> sourceUsage,
        IReadOnlyDictionary<string, HashSet<string>> targetUsage)
        => schemas.OnlyInSource.SelectMany(schema =>
                GetDirections(schema.Name, null, sourceUsage, targetUsage).Select(direction =>
                    SpecDifferenceFactory.OnlyInSource(
                        DifferenceKindCodes.SchemaRemoved,
                        direction,
                        new FindingAddress(schemaName: schema.Name),
                        schema.Name)))
            .Concat(schemas.OnlyInTarget.SelectMany(schema =>
                GetDirections(null, schema.Name, sourceUsage, targetUsage).Select(direction =>
                    SpecDifferenceFactory.OnlyInTarget(
                        DifferenceKindCodes.SchemaAdded,
                        direction,
                        new FindingAddress(schemaName: schema.Name),
                        schema.Name))))
            .ToList();

    // Farkli adla eslesen semalari sil-ekle cifti yerine tek rename bulgusuna cevirir.
    private static List<SpecDifferenceModel> CompareSchemaRenames(
        IEnumerable<(SpecSchemaModel Source, SpecSchemaModel Target)> pairs,
        IReadOnlyDictionary<string, HashSet<string>> sourceUsage,
        IReadOnlyDictionary<string, HashSet<string>> targetUsage)
        => pairs
            .Where(pair => !string.Equals(pair.Source.Name, pair.Target.Name, StringComparison.Ordinal))
            .SelectMany(pair => GetDirections(pair.Source.Name, pair.Target.Name, sourceUsage, targetUsage)
                .Select(direction => SpecDifferenceFactory.Modified(
                    DifferenceKindCodes.SchemaRenamed,
                    direction,
                    new FindingAddress(schemaName: pair.Source.Name),
                    pair.Source.Name,
                    pair.Target.Name)))
            .ToList();

    // Eslesen her semanin request ve response property kurallarini yalniz erisildigi yonlerde calistirir.
    private List<SpecDifferenceModel> CompareMatchedProperties(
        IEnumerable<(SpecSchemaModel Source, SpecSchemaModel Target)> pairs,
        IReadOnlyDictionary<string, HashSet<string>> sourceUsage,
        IReadOnlyDictionary<string, HashSet<string>> targetUsage)
    {
        var differences = new List<SpecDifferenceModel>();
        foreach (var (source, target) in pairs)
        {
            var directions = GetDirections(source.Name, target.Name, sourceUsage, targetUsage);
            if (directions.Contains(DifferenceDirectionCodes.Request, StringComparer.Ordinal))
            {
                differences.AddRange(CompareRequestProperties(source, target));
            }

            if (directions.Contains(DifferenceDirectionCodes.Response, StringComparer.Ordinal))
            {
                differences.AddRange(CompareResponseProperties(source, target));
            }
        }

        return differences;
    }

    // Request semasinda yeni zorunlu alan, zorunluluk ve tip degisimlerini uretir.
    private List<SpecDifferenceModel> CompareRequestProperties(
        SpecSchemaModel sourceSchema,
        SpecSchemaModel targetSchema)
    {
        var differences = _collectionComparer.Compare(
            sourceSchema.Properties,
            targetSchema.Properties.Where(property => property.Required),
            property => property.Name,
            property => new FindingAddress(schemaName: targetSchema.Name, propertyPath: property.Name),
            property => property.Name,
            _ => SpecComparisonTextConstants.Required,
            (source, target) => (source.Name, target.Name),
            null,
            DifferenceKindCodes.NewRequiredRequestProperty,
            null,
            DifferenceDirectionCodes.Request);

        foreach (var (source, target) in _collectionComparer.EnumerateMatched(
                     sourceSchema.Properties,
                     targetSchema.Properties,
                     property => property.Name))
        {
            var address = new FindingAddress(schemaName: sourceSchema.Name, propertyPath: source.Name);
            if (!source.Required && target.Required)
            {
                differences.Add(SpecDifferenceFactory.Modified(
                    DifferenceKindCodes.RequestPropertyBecameRequired,
                    DifferenceDirectionCodes.Request,
                    address,
                    SpecComparisonTextConstants.Optional,
                    SpecComparisonTextConstants.Required));
            }

            if (!string.Equals(source.Type, target.Type, StringComparison.Ordinal))
            {
                differences.Add(SpecDifferenceFactory.Modified(
                    DifferenceKindCodes.RequestPropertyTypeChanged,
                    DifferenceDirectionCodes.Request,
                    address,
                    source.Type,
                    target.Type));
            }
        }

        return differences;
    }

    // Response semasinda alan kaybi, opsiyonellesme, nullable genislemesi ve tip degisimini uretir.
    private List<SpecDifferenceModel> CompareResponseProperties(
        SpecSchemaModel sourceSchema,
        SpecSchemaModel targetSchema)
    {
        var differences = _collectionComparer.Compare(
            sourceSchema.Properties.Where(property => property.Required),
            targetSchema.Properties,
            property => property.Name,
            property => new FindingAddress(schemaName: sourceSchema.Name, propertyPath: property.Name),
            property => property.Name,
            _ => SpecComparisonTextConstants.Required,
            (source, target) => (source.Name, target.Name),
            DifferenceKindCodes.ResponsePropertyBecameOptional,
            null,
            null,
            DifferenceDirectionCodes.Response);

        foreach (var (source, target) in _collectionComparer.EnumerateMatched(
                     sourceSchema.Properties,
                     targetSchema.Properties,
                     property => property.Name))
        {
            var address = new FindingAddress(schemaName: sourceSchema.Name, propertyPath: source.Name);
            if (source.Required && !target.Required)
            {
                differences.Add(SpecDifferenceFactory.Modified(
                    DifferenceKindCodes.ResponsePropertyBecameOptional,
                    DifferenceDirectionCodes.Response,
                    address,
                    SpecComparisonTextConstants.Required,
                    SpecComparisonTextConstants.Optional));
            }

            if (!source.Nullable && target.Nullable)
            {
                differences.Add(SpecDifferenceFactory.Modified(
                    DifferenceKindCodes.ResponsePropertyBecameNullable,
                    DifferenceDirectionCodes.Response,
                    address,
                    SpecComparisonTextConstants.NonNullable,
                    SpecComparisonTextConstants.Nullable));
            }

            if (!string.Equals(source.Type, target.Type, StringComparison.Ordinal))
            {
                differences.Add(SpecDifferenceFactory.Modified(
                    DifferenceKindCodes.RequestPropertyTypeChanged,
                    DifferenceDirectionCodes.Response,
                    address,
                    source.Type,
                    target.Type));
            }
        }

        return differences;
    }

    // Iki snapshot'taki kullanimlari birlestirip sema cifti icin request/response yonlerini bulur.
    private static IReadOnlyCollection<string> GetDirections(
        string? sourceSchemaName,
        string? targetSchemaName,
        IReadOnlyDictionary<string, HashSet<string>> sourceUsage,
        IReadOnlyDictionary<string, HashSet<string>> targetUsage)
    {
        var sourceDirections = sourceSchemaName is not null &&
                               sourceUsage.TryGetValue(sourceSchemaName, out var foundSource)
            ? foundSource
            : Enumerable.Empty<string>();
        var targetDirections = targetSchemaName is not null &&
                               targetUsage.TryGetValue(targetSchemaName, out var foundTarget)
            ? foundTarget
            : Enumerable.Empty<string>();
        return sourceDirections
            .Concat(targetDirections)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    // Operasyon koklerinden property referanslarini da izleyerek her semanin request/response kullanimini cozer.
    private static Dictionary<string, HashSet<string>> ResolveUsage(SpecSnapshotModel snapshot)
    {
        var schemas = snapshot.Schemas
            .GroupBy(schema => schema.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var usage = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        // Tek sema referansini ve property referans zincirini donguleri keserek ziyaret eder.
        void Visit(string? schemaName, string direction)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                return;
            }

            if (!usage.TryGetValue(schemaName, out var directions))
            {
                directions = new HashSet<string>(StringComparer.Ordinal);
                usage[schemaName] = directions;
            }

            if (!directions.Add(direction) || !schemas.TryGetValue(schemaName, out var schema))
            {
                return;
            }

            Visit(schema.ReferenceId, direction);
            foreach (var property in schema.Properties)
            {
                Visit(property.ReferenceId, direction);
            }
        }

        foreach (var operation in snapshot.Operations)
        {
            foreach (var body in operation.RequestBodies)
            {
                Visit(body.SchemaReferenceId, DifferenceDirectionCodes.Request);
            }

            foreach (var response in operation.Responses)
            {
                Visit(response.SchemaReferenceId, DifferenceDirectionCodes.Response);
            }
        }

        return usage;
    }
}
