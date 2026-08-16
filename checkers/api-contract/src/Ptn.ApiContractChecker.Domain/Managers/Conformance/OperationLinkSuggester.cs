using System.Text.Json.Nodes;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Beyan edilmis link, kesin sema eslesmesi ve Location orneginden sonraki operasyon adaylari uretir.
// sistemdeki gorevi: Esik alti veya kanitsiz tahmini elemeden gecirip karari her zaman insana birakir.
public sealed class OperationLinkSuggester : ITransientDependency
{
    private readonly ISpecSchemaResolver _schemaResolver;
    private readonly OperationResolver _operationResolver;

    public OperationLinkSuggester(ISpecSchemaResolver schemaResolver, OperationResolver operationResolver)
    {
        _schemaResolver = schemaResolver;
        _operationResolver = operationResolver;
    }

    // Kaynak operasyonun uc mekanik kanit ailesindeki esik ustu adaylarini guven sirasinda dondurur.
    public async Task<OperationLinkResult> SuggestAsync(
        SpecSnapshot? snapshot,
        OperationLinkRequest request)
    {
        if (snapshot?.SpecContent == null)
        {
            return Empty(ConformanceOutcomeCodes.SnapshotNotFound);
        }

        var model = await _schemaResolver.GetSnapshotAsync(snapshot.SpecContent);
        var source = _operationResolver.Resolve(model, request.SourceOperationId, string.Empty, string.Empty);
        if (source == null)
        {
            return Empty(ConformanceOutcomeCodes.OperationNotResolved);
        }

        var candidates = BuildCandidates(model, source)
            .Where(candidate => candidate.Score >= SampleGenerationConsts.LinkScoreThreshold)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.TargetOperationId, StringComparer.Ordinal)
            .GroupBy(candidate => candidate.TargetOperationId, StringComparer.Ordinal)
            .Select(group => group.First())
            .Take(request.MaxCandidates)
            .ToList();
        return new OperationLinkResult(ConformanceOutcomeCodes.Passed, candidates);
    }

    // Uc kanit kaynaginin adaylarini ayni listede toplar.
    private List<OperationLinkCandidate> BuildCandidates(
        SpecSnapshotModel snapshot,
        SpecOperationModel source)
    {
        var candidates = BuildDeclaredCandidates(snapshot, source);
        candidates.AddRange(BuildSchemaCandidates(snapshot, source));
        candidates.AddRange(BuildLocationCandidates(snapshot, source));
        return candidates;
    }

    // OpenAPI links beyanlarini en yuksek guvenli adaylara cevirir.
    private List<OperationLinkCandidate> BuildDeclaredCandidates(
        SpecSnapshotModel snapshot,
        SpecOperationModel source)
    {
        return source.Responses
            .SelectMany(response => response.Links)
            .Select(link => new { Link = link, Target = ResolveDeclaredTarget(snapshot, link) })
            .Where(item => item.Target?.OperationId != null)
            .Select(item => new OperationLinkCandidate(
                item.Target!.OperationId!,
                OperationLinkSourceCodes.DeclaredLink,
                BuildDeclaredBindings(item.Link, item.Target),
                SampleGenerationConsts.DeclaredLinkScore))
            .ToList();
    }

    // Link operationId veya operationRef degerini snapshot'taki tek hedefe cozer.
    private SpecOperationModel? ResolveDeclaredTarget(
        SpecSnapshotModel snapshot,
        SpecOperationLinkModel link)
    {
        if (!string.IsNullOrWhiteSpace(link.TargetOperationId))
        {
            return _operationResolver.Resolve(snapshot, link.TargetOperationId, string.Empty, string.Empty);
        }

        return ResolveOperationReference(snapshot, link.TargetOperationReference);
    }

    // Beyan edilmis response expression'larini gercek hedef parametrelerine baglar.
    private static List<OperationLinkParameterBinding> BuildDeclaredBindings(
        SpecOperationLinkModel link,
        SpecOperationModel target)
    {
        return link.ParameterExpressions
            .Where(expression => target.Parameters.Any(parameter =>
                string.Equals(parameter.Name, expression.Key, StringComparison.Ordinal)))
            .Select(expression => new
            {
                expression.Key,
                Pointer = BuildSourcePointer(expression.Value)
            })
            .Where(item => item.Pointer != null)
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new OperationLinkParameterBinding(item.Pointer!, item.Key))
            .ToList();
    }

    // Source 2xx response alanlarini hedef parametre ad ve tipiyle kesin eslestirir.
    private static List<OperationLinkCandidate> BuildSchemaCandidates(
        SpecSnapshotModel snapshot,
        SpecOperationModel source)
    {
        var sourceFields = source.Responses
            .Where(IsSuccessful)
            .SelectMany(response => response.Schema?.Properties ?? [])
            .DistinctBy(property => string.Concat(property.Name, ResolveType(property)))
            .ToList();

        return snapshot.Operations
            .Where(target => !ReferenceEquals(target, source) && target.OperationId != null)
            .Select(target => new
            {
                Target = target,
                Bindings = BuildSchemaBindings(sourceFields, target.Parameters)
            })
            .Where(item => item.Bindings.Count > 0)
            .Select(item => new OperationLinkCandidate(
                item.Target.OperationId!,
                OperationLinkSourceCodes.SchemaMatch,
                item.Bindings,
                SampleGenerationConsts.SchemaMatchScore))
            .ToList();
    }

    // Ayni ad ve tipe sahip source property ile hedef parametreleri pointer eslemelerine cevirir.
    private static List<OperationLinkParameterBinding> BuildSchemaBindings(
        IEnumerable<SpecSchemaPropertyModel> sourceFields,
        IEnumerable<SpecParameterModel> targetParameters)
    {
        return sourceFields
            .SelectMany(source => targetParameters
                .Where(target => string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase) &&
                                 string.Equals(ResolveType(source), ResolveType(target),
                                     StringComparison.OrdinalIgnoreCase))
                .Select(target => new OperationLinkParameterBinding(
                    BuildBodyPointer(source.Name),
                    target.Name)))
            .OrderBy(binding => binding.TargetParameterName, StringComparer.Ordinal)
            .ToList();
    }

    // 201 Location string orneginin tekil olarak isaret ettigi operasyonu aday yapar.
    private List<OperationLinkCandidate> BuildLocationCandidates(
        SpecSnapshotModel snapshot,
        SpecOperationModel source)
    {
        return source.Responses
            .Where(response => response.StatusCode == SampleGenerationConsts.CreatedStatusCode)
            .SelectMany(response => response.Headers)
            .Where(header => string.Equals(
                header.Name,
                SampleGenerationConsts.LocationHeaderName,
                StringComparison.OrdinalIgnoreCase))
            .Select(header => ResolveLocationTarget(snapshot, source, ReadStringExample(header.Example)))
            .Where(target => target?.OperationId != null)
            .Select(target => new OperationLinkCandidate(
                target!.OperationId!,
                OperationLinkSourceCodes.LocationHeader,
                [],
                SampleGenerationConsts.LocationHeaderScore))
            .ToList();
    }

    // Location yolunu tum bildirilen methodlarda cozer ve yalniz tek hedef varsa kabul eder.
    private SpecOperationModel? ResolveLocationTarget(
        SpecSnapshotModel snapshot,
        SpecOperationModel source,
        string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        var targets = snapshot.Operations
            .Where(operation => !ReferenceEquals(operation, source))
            .Select(operation => _operationResolver.Resolve(snapshot, null, operation.Method, location))
            .Where(operation => operation != null)
            .Distinct()
            .ToList();
        return targets.Count == 1 ? targets[0] : null;
    }

    // JSON string header ornegini Location yolu olarak okur; diger tipleri reddeder.
    private static string? ReadStringExample(string? example)
    {
        if (string.IsNullOrWhiteSpace(example))
        {
            return null;
        }

        return JsonNode.Parse(example) is JsonValue value && value.TryGetValue<string>(out var text)
            ? text
            : null;
    }

    // Standart #/paths/{pointer}/{method} operationRef degerini mevcut resolver ile cozer.
    private SpecOperationModel? ResolveOperationReference(
        SpecSnapshotModel snapshot,
        string? operationReference)
    {
        var prefixIndex = operationReference?.IndexOf(
            SampleGenerationConsts.OperationReferencePathsPrefix,
            StringComparison.Ordinal) ?? -1;
        if (prefixIndex < 0)
        {
            return null;
        }

        var reference = operationReference![(prefixIndex + SampleGenerationConsts.OperationReferencePathsPrefix.Length)..];
        var methodSeparator = reference.LastIndexOf(ConformanceTextConstants.PathSeparator);
        if (methodSeparator <= 0 || methodSeparator == reference.Length - 1)
        {
            return null;
        }

        var path = DecodePointerSegment(reference[..methodSeparator]);
        var method = reference[(methodSeparator + 1)..];
        return _operationResolver.Resolve(snapshot, null, method, path);
    }

    // JSON Pointer kacisini operationRef path metnine geri acar.
    private static string DecodePointerSegment(string value)
    {
        return Uri.UnescapeDataString(value)
            .Replace(ConformanceTextConstants.JsonPointerEscapedSlash,
                ConformanceTextConstants.JsonPointerSeparator, StringComparison.Ordinal)
            .Replace(ConformanceTextConstants.JsonPointerEscapedTilde,
                ConformanceTextConstants.JsonPointerTilde, StringComparison.Ordinal);
    }

    // Yalniz response body veya header runtime expression'ini public response pointer'ina cevirir.
    private static string? BuildSourcePointer(string expression)
    {
        if (expression.StartsWith(SampleGenerationConsts.ResponseBodyExpressionPrefix, StringComparison.Ordinal))
        {
            var pointer = expression[SampleGenerationConsts.ResponseBodyExpressionPrefix.Length..];
            return string.Concat(BuildPointer(ConformanceAuthoringConstants.BodyPointerSegment), pointer);
        }

        if (expression.StartsWith(SampleGenerationConsts.ResponseHeaderExpressionPrefix, StringComparison.Ordinal))
        {
            return BuildPointer(
                ConformanceTextConstants.HeadersPointerSegment,
                expression[SampleGenerationConsts.ResponseHeaderExpressionPrefix.Length..]);
        }

        return null;
    }

    // Response property adini RFC 6901 body pointer'ina cevirir.
    private static string BuildBodyPointer(string name)
    {
        return BuildPointer(ConformanceAuthoringConstants.BodyPointerSegment, name);
    }

    // Pointer segmentlerini RFC 6901 kacisiyla birlestirir.
    private static string BuildPointer(params string[] segments)
    {
        return string.Concat(segments.Select(segment => string.Concat(
            ConformanceTextConstants.JsonPointerSeparator,
            segment.Replace(ConformanceTextConstants.JsonPointerTilde,
                    ConformanceTextConstants.JsonPointerEscapedTilde, StringComparison.Ordinal)
                .Replace(ConformanceTextConstants.JsonPointerSeparator,
                    ConformanceTextConstants.JsonPointerEscapedSlash, StringComparison.Ordinal))));
    }

    // Yalniz 2xx response'lari sema kaynagi kabul eder.
    private static bool IsSuccessful(SpecResponseModel response)
    {
        return int.TryParse(response.StatusCode, out var statusCode) && statusCode is >= 200 and <= 299;
    }

    // Property'nin tam semasini, yoksa geriye uyumlu duz tipini okur.
    private static string? ResolveType(SpecSchemaPropertyModel property)
    {
        return property.Schema?.Type ?? property.Type;
    }

    // Parametrenin tam semasini, yoksa geriye uyumlu duz tipini okur.
    private static string? ResolveType(SpecParameterModel parameter)
    {
        return parameter.Schema?.Type ?? parameter.Type;
    }

    // Snapshot veya kaynak operasyon bulunamadiginda acik outcome ile bos sonuc kurar.
    private static OperationLinkResult Empty(string outcomeCode)
    {
        return new OperationLinkResult(outcomeCode, []);
    }
}
