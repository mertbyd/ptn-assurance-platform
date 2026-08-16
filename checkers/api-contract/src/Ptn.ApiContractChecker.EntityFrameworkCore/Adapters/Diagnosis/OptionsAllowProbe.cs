using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Diagnosis;

// islevi: Belgeli hedefe OPTIONS yapip Allow header'ini yapilandirilmis kanita cevirir.
// sistemdeki gorevi: Ortamda desteklenen method yuzeyini SUT durumunu degistirmeden spec olgusuyla karsilastirmaya verir.
public sealed class OptionsAllowProbe : SafeHttpDiagnosisProbeBase, ITransientDependency
{
    public override string ProbeKindCode => ProbeKindCodes.OptionsAllow;
    protected override HttpMethod SafeMethod { get; } = new(DiagnosisHttpConstants.Options);

    public OptionsAllowProbe(IHttpClientFactory factory, ProbeTargetGuard guard) : base(factory, guard)
    {
    }

    // islevi: Allow header degerlerini ordinal sirali tek kanit degerine indirger.
    protected override ProbeEvidence BuildEvidence(
        ProbeRequest request,
        HttpResponseMessage response,
        long observedAtMs)
    {
        var allow = response.Headers.TryGetValues(DiagnosisHttpConstants.Allow, out var values)
            ? values.OrderBy(item => item, StringComparer.Ordinal).ToList()
            : new List<string>();
        var evidence = CreateEvidence(request,
            allow.Count > 0 ? ProbeKindCodes.Facts.Present : ProbeKindCodes.Facts.Absent,
            observedAtMs);
        evidence.ObservedValue = string.Join(",", allow);
        return evidence;
    }
}
