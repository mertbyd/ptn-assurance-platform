using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Gozlenen method/path veya operationId'yi snapshot'taki tek operasyona cozer.
// sistemdeki gorevi: Path sablonu ve server prefix belirsizliginde tahmin yapmadan assertion'i kapatir.
public class OperationResolver : ITransientDependency
{
    public SpecOperationModel? Resolve(
        SpecSnapshotModel snapshot,
        string? operationId,
        string method,
        string path)
    {
        var candidates = string.IsNullOrWhiteSpace(operationId)
            ? ResolveByMethodAndPath(snapshot, method, path)
            : snapshot.Operations.Where(operation => operation.OperationId == operationId).ToList();
        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static List<SpecOperationModel> ResolveByMethodAndPath(
        SpecSnapshotModel snapshot,
        string method,
        string path)
    {
        var observedPaths = BuildObservedPaths(snapshot.Servers, path);
        return snapshot.Operations
            .Where(operation => string.Equals(operation.Method, method, StringComparison.OrdinalIgnoreCase))
            .Where(operation => observedPaths.Any(candidate => PathMatches(operation.Path, candidate)))
            .Distinct()
            .ToList();
    }

    // Mutlak URL, query ve birden cok server prefix'inden olasi path'leri uretir; secimi operasyon tekilligi yapar.
    private static List<string> BuildObservedPaths(IEnumerable<string> servers, string path)
    {
        var rawPath = ExtractPath(path);
        var paths = new HashSet<string>(StringComparer.Ordinal) { rawPath };
        foreach (var server in servers)
        {
            var prefix = ExtractPath(server).TrimEnd(ConformanceTextConstants.PathSeparator);
            if (prefix.Length > 0 && rawPath.StartsWith(prefix, StringComparison.Ordinal))
            {
                paths.Add(rawPath[prefix.Length..]);
            }
        }

        return paths.Select(EnsureLeadingSlash).ToList();
    }

    private static string ExtractPath(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return uri.AbsolutePath;
        }

        var queryIndex = value.IndexOf(ConformanceTextConstants.QuerySeparator);
        return queryIndex < 0 ? value : value[..queryIndex];
    }

    private static string EnsureLeadingSlash(string path)
    {
        var trimmed = path.Trim();
        return trimmed.StartsWith(ConformanceTextConstants.PathSeparator)
            ? trimmed
            : string.Concat(ConformanceTextConstants.JsonPointerSeparator, trimmed);
    }

    private static bool PathMatches(string template, string observed)
    {
        var templateSegments = SplitPath(template);
        var observedSegments = SplitPath(observed);
        return templateSegments.Length == observedSegments.Length &&
               templateSegments.Zip(observedSegments).All(pair => SegmentMatches(pair.First, pair.Second));
    }

    private static string[] SplitPath(string path)
    {
        return path.Split(ConformanceTextConstants.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool SegmentMatches(string template, string observed)
    {
        var isParameter = template.Length > 1 &&
                          template[0] == ConformanceTextConstants.TemplateStart &&
                          template[^1] == ConformanceTextConstants.TemplateEnd;
        return isParameter ? observed.Length > 0 : template == observed;
    }
}
