using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Identity;

// islevi: RFC 9457 ve ABP RemoteServiceErrorInfo yapilandirilmis alanlarini kimlige tasir.
// sistemdeki gorevi: Lokalize veya yapilandirilmamis response govdesini parse etmeden problem olgularini korur.
public sealed class ProblemDetailsIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    public int Priority => 300;

    // islevi: Sinyalde RFC 9457 veya ABP hata alani bulunup bulunmadigini bildirir.
    public bool CanExtract(HttpFailureSignal signal)
        => !string.IsNullOrWhiteSpace(signal.ProblemType) ||
           !string.IsNullOrWhiteSpace(signal.RemoteServiceErrorCode) ||
           signal.ProblemErrors.Count > 0;

    // islevi: Mutlak problem type veya ABP kodunu yuksek guvenli yapilandirilmis kimlige cevirir.
    public void Extract(HttpFailureSignal signal, SpecSnapshotModel snapshot, FailureIdentity identity)
    {
        if (!string.IsNullOrWhiteSpace(signal.ProblemType) &&
            !Uri.TryCreate(signal.ProblemType, UriKind.Absolute, out _))
        {
            identity.RejectStructuredName();
            return;
        }

        identity.ProblemType = signal.ProblemType;
        identity.ProblemTitle = signal.ProblemTitle;
        identity.ProblemDetail = signal.ProblemDetail;
        identity.ProblemInstance = signal.ProblemInstance;
        identity.RemoteServiceErrorCode = signal.RemoteServiceErrorCode;
        identity.ProblemErrors = signal.ProblemErrors.ToList();
        identity.Upgrade();
    }
}
