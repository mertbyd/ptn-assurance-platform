using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Diagnosis;

// islevi: Allow-list'teki auth metadata hedefini GET ile yoklayip govde okumadan erisilebilirlik kaniti uretir.
// sistemdeki gorevi: Issuer veya protected-resource metadata kesintisini token degeri tasimadan ayirmaya yardim eder.
public sealed class AuthMetadataProbe : SafeHttpDiagnosisProbeBase, ITransientDependency
{
    public override string ProbeKindCode => ProbeKindCodes.AuthMetadata;
    protected override HttpMethod SafeMethod => HttpMethod.Get;

    public AuthMetadataProbe(IHttpClientFactory factory, ProbeTargetGuard guard) : base(factory, guard)
    {
    }

    // islevi: Basarili metadata statusunu yapilandirilmis reachable kanitina cevirir.
    protected override ProbeEvidence BuildEvidence(
        ProbeRequest request,
        HttpResponseMessage response,
        long observedAtMs)
        => CreateEvidence(request,
            response.IsSuccessStatusCode ? ProbeKindCodes.Facts.Reachable : ProbeKindCodes.Facts.Unreachable,
            observedAtMs);
}
