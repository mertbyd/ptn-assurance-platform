using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Diagnosis;

// islevi: Snapshot server kokune GET yapip servis erisilebilirlik olgusunu toplar.
// sistemdeki gorevi: Transport kesintisi ile tek endpoint deploy farkini safe cagriyla ayirmaya yardim eder.
public sealed class ServerReachabilityProbe : SafeHttpDiagnosisProbeBase, ITransientDependency
{
    public override string ProbeKindCode => ProbeKindCodes.ServerReachability;
    protected override HttpMethod SafeMethod => HttpMethod.Get;

    public ServerReachabilityProbe(IHttpClientFactory factory, ProbeTargetGuard guard) : base(factory, guard)
    {
    }

    // islevi: Her HTTP response'u server ulasilabilir, tasima exception'ini butce yoneticisine birakir.
    protected override ProbeEvidence BuildEvidence(
        ProbeRequest request,
        HttpResponseMessage response,
        long observedAtMs)
        => CreateEvidence(request, ProbeKindCodes.Facts.Reachable, observedAtMs);
}
