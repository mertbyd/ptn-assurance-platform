using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Snapshot info.version ile response surum metadata farkini degerlendirir.
// sistemdeki gorevi: H-EN-04'u tarih veya deploy tahmini yerine iki yapilandirilmis surum olgusuna baglar.
public sealed class SnapshotVersionMismatchRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.SnapshotVersionMismatch;
    public override int Priority => 67;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => context.Snapshot.ApiVersion != null && context.Signal.ResponseVersion != null;
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => [CreateProbe(ProbeKindCodes.SnapshotFreshness, context)];

    // islevi: Surum mismatch'te Confirmed, match'te RuledOut, eksik kanitta Possible sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = FindEvidence(evidence, ProbeKindCodes.SnapshotFreshness);
        var confidence = proof?.FactCode switch
        {
            ProbeKindCodes.Facts.Mismatch => DiagnosisConfidenceCodes.Confirmed,
            ProbeKindCodes.Facts.Match => DiagnosisConfidenceCodes.RuledOut,
            _ => DiagnosisConfidenceCodes.Possible
        };
        return CreateAssessment(confidence, proof is null ? [] : [proof]);
    }
}
