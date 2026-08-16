using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Diagnosis;

// islevi: Snapshot path sablonuna uyan kaynak URL'ine HEAD yapip kaynagin gorunurlugunu olcer.
// sistemdeki gorevi: Kaynak hic olusmadi ve gec olustu hipotezlerine yazmasiz network kaniti saglar.
public sealed class HeadResourceProbe : SafeHttpDiagnosisProbeBase, ITransientDependency
{
    public override string ProbeKindCode => ProbeKindCodes.HeadResource;
    protected override HttpMethod SafeMethod => HttpMethod.Head;

    public HeadResourceProbe(IHttpClientFactory factory, ProbeTargetGuard guard) : base(factory, guard)
    {
    }

    // islevi: Basarili HEAD durumunu kaynak var olgusu olarak kodlar.
    protected override ProbeEvidence BuildEvidence(
        ProbeRequest request,
        HttpResponseMessage response,
        long observedAtMs)
        => CreateEvidence(request,
            response.IsSuccessStatusCode ? ProbeKindCodes.Facts.Present : ProbeKindCodes.Facts.Absent,
            observedAtMs);
}
