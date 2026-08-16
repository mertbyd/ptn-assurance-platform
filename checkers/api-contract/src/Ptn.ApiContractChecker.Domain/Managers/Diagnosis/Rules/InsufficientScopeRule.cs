using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Challenge scope'larini operasyon SecurityRequirements scope'lariyla karsilastirir.
// sistemdeki gorevi: H-AU-03'u RFC 6750 ve canli snapshot olgularindan deterministik olarak uretir.
public sealed class InsufficientScopeRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.InsufficientScope;
    public override int Priority => 76;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.ChallengeScheme != null && context.Operation?.SecurityRequirements.Count > 0;

    // islevi: Operasyon scope kapsamasini agsiz SpecFact probe'una devreder.
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context)
        => [CreateProbe(ProbeKindCodes.SpecFact, context, ProbeKindCodes.Names.SecurityScope)];

    // islevi: Scope kapsanmiyorsa veya challenge insufficient_scope diyorsa Confirmed sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proof = FindEvidence(evidence, ProbeKindCodes.SpecFact);
        var insufficient = identity.ChallengeError == DiagnosisHttpConstants.InsufficientScope ||
                           proof?.FactCode == ProbeKindCodes.Facts.Absent;
        return CreateAssessment(insufficient ? DiagnosisConfidenceCodes.Confirmed : DiagnosisConfidenceCodes.RuledOut,
            proof is null ? [] : [proof]);
    }
}
