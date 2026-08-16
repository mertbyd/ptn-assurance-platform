using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Undocumented property conformance sonucunu sozlesme disi assertion hipotezine cevirir.
// sistemdeki gorevi: H-AS-03'u kapali KBP-621 outcome kodundan deterministik olarak uretir.
public sealed class AssertionOutsideContractRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.AssertionOutsideContract;
    public override int Priority => 56;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.ConformanceOutcomeCode == ConformanceOutcomeCodes.UndocumentedProperty;
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context) => [];

    // islevi: Kapali undocumented-property sonucunu Confirmed assessment'e cevirir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => CreateAssessment(DiagnosisConfidenceCodes.Confirmed);
}
