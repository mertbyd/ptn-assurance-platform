using Ptn.ApiContractChecker.ExceptionCodes.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis;

// islevi: Network probe hedefini snapshot server origin'i ve belgeli path sablonuna karsi dogrular.
// sistemdeki gorevi: Ajan, response govdesi veya Location header'indan gelebilecek keyfi URL'leri adapterden once reddeder.
public sealed class ProbeTargetGuard : ITransientDependency
{
    // islevi: Mutlak HTTP(S) hedefinin izinli server kokunde ve gerekiyorsa belgeli path'te oldugunu garanti eder.
    public void EnsureAllowed(ProbeRequest request)
    {
        if (request.TargetUri is null || !IsHttp(request.TargetUri))
        {
            ThrowUnsafeTarget();
        }

        var server = request.AllowedServerUrls
            .Select(ParseServer)
            .FirstOrDefault(item => item != null && HasSameOrigin(item, request.TargetUri!));
        if (server is null || !IsAllowedPath(server, request.TargetUri!, request.SpecPaths))
        {
            ThrowUnsafeTarget();
        }
    }

    // islevi: Yalniz HTTP ve HTTPS semalarini network probe icin kabul eder.
    private static bool IsHttp(Uri uri)
        => uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    // islevi: Snapshot server metnini guvenli mutlak URI'ye cevirir.
    private static Uri? ParseServer(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && IsHttp(uri) ? uri : null;

    // islevi: Scheme, host ve etkin port eslesmesini origin esitligi olarak uygular.
    private static bool HasSameOrigin(Uri server, Uri target)
        => string.Equals(server.Scheme, target.Scheme, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(server.Host, target.Host, StringComparison.OrdinalIgnoreCase) &&
           server.Port == target.Port;

    // islevi: Server kokunu veya server prefix'i altindaki belgeli path sablonunu kabul eder.
    private static bool IsAllowedPath(Uri server, Uri target, IReadOnlyCollection<string> specPaths)
    {
        var serverPath = NormalizePath(server.AbsolutePath);
        var targetPath = NormalizePath(target.AbsolutePath);
        if (targetPath == serverPath)
        {
            return true;
        }

        return specPaths.Any(path => MatchesTemplate(
            targetPath, NormalizePath(string.Concat(serverPath.TrimEnd('/'), "/", path.TrimStart('/')))));
    }

    // islevi: Path sablonundaki yalniz suslu parantezli segmentleri tek segment wildcard olarak esler.
    private static bool MatchesTemplate(string actual, string template)
    {
        var actualSegments = actual.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var templateSegments = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return actualSegments.Length == templateSegments.Length &&
               actualSegments.Zip(templateSegments).All(pair => IsTemplateSegment(pair.Second) ||
                   string.Equals(pair.First, pair.Second, StringComparison.Ordinal));
    }

    // islevi: OpenAPI path parametresi segmentini tanir.
    private static bool IsTemplateSegment(string segment)
        => segment.Length > 2 && segment[0] == '{' && segment[^1] == '}';

    // islevi: Path karsilastirmasinda kok ve sondaki slash farkini kanoniklestirir.
    private static string NormalizePath(string path)
        => string.Concat("/", path.Trim('/'));

    // islevi: Guvenli hedef invariant'i bozuldugunda kararli ABP hatasi uretir.
    private static void ThrowUnsafeTarget()
        => throw new BusinessException(DiagnosisExceptionCodes.UnsafeProbeTarget);
}
