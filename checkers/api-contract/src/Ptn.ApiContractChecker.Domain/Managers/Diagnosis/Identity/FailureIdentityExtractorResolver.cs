using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Identity;

// islevi: Sinyale uygulanabilen extractor'lari oncelik ve tip adiyla deterministik sirada birlestirir.
// sistemdeki gorevi: Kaynak secimini if/switch zincirinden ayirir; her yapilandirilmis alan ailesini kendi sinifinda tutar.
public sealed class FailureIdentityExtractorResolver : ITransientDependency
{
    private readonly List<IFailureIdentityExtractor> _extractors;

    public FailureIdentityExtractorResolver(IEnumerable<IFailureIdentityExtractor> extractors)
    {
        _extractors = extractors
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.GetType().FullName, StringComparer.Ordinal)
            .ToList();
    }

    // islevi: Temel HTTP kimligini kurup uygulanabilir extractor olgularini tek kimlikte birlestirir.
    public FailureIdentity Extract(HttpFailureSignal signal, SpecSnapshotModel snapshot)
    {
        var identity = CreateBaseIdentity(signal);
        foreach (var extractor in _extractors.Where(item => item.CanExtract(signal)))
        {
            extractor.Extract(signal, snapshot, identity);
        }

        return identity;
    }

    // islevi: Sinyalin kaynak ve durum sinifi gibi extractor-bagimsiz olgularini kurar.
    private static FailureIdentity CreateBaseIdentity(HttpFailureSignal signal)
    {
        return new FailureIdentity
        {
            SourceKindCode = ResolveSourceKind(signal),
            StatusCode = signal.StatusCode,
            StatusClassCode = HttpStatusClassCodes.FromStatusCode(signal.StatusCode),
            SentContentType = signal.SentContentType,
            ObjectReferences =
            [
                new ObjectReference
                {
                    OperationId = signal.OperationId,
                    Method = signal.Method?.ToUpperInvariant(),
                    Path = signal.Path
                }
            ]
        };
    }

    // islevi: Kaynak turunu conformance, HTTP ve transport onceliginde kapali koda cevirir.
    private static string ResolveSourceKind(HttpFailureSignal signal)
    {
        if (!string.IsNullOrWhiteSpace(signal.ConformanceOutcomeCode))
        {
            return FailureSourceKindCodes.Conformance;
        }

        return signal.StatusCode.HasValue
            ? FailureSourceKindCodes.HttpStatus
            : FailureSourceKindCodes.Transport;
    }
}
