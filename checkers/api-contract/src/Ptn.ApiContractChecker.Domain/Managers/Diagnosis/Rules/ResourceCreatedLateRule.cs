using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Basarisizliktan sonra HEAD ile gorunen kaynagi gec olustu hipotezi olarak degerlendirir.
// sistemdeki gorevi: H-ST-02'yi ayni safe resource olcumunden bagimsiz assessment olarak uretir.
public sealed class ResourceCreatedLateRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.ResourceCreatedLate;
    public override int Priority => 80;

    // islevi: Resource URL ve gozlem zamani bulunan sinyali aday yapar.
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.Signal.ObservedAtMs.HasValue &&
           Uri.TryCreate(context.Signal.ResourceUrl, UriKind.Absolute, out _);

    // islevi: Gec gorunurlugu olcmek icin fixed HEAD probe istegi uretir.
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => [CreateProbe(ProbeKindCodes.HeadResource, context, targetUri: new Uri(context.Signal.ResourceUrl!))];

    // islevi: Kaynak simdi varsa Confirmed, halen yoksa RuledOut, probe yoksa Possible sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = FindEvidence(evidence, ProbeKindCodes.HeadResource);
        var confidence = proof?.FactCode switch
        {
            ProbeKindCodes.Facts.Present => DiagnosisConfidenceCodes.Confirmed,
            ProbeKindCodes.Facts.Absent => DiagnosisConfidenceCodes.RuledOut,
            _ => DiagnosisConfidenceCodes.Possible
        };
        return CreateAssessment(confidence, proof is null ? [] : [proof]);
    }
}
