using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Schema assertion hatasinda kararsiz alan literal assert edilmis olma olasiligini gorunur tutar.
// sistemdeki gorevi: H-AS-04'u olcum kaniti olmadigi surece yalniz Possible seviyesinde raporlar.
public sealed class VolatileLiteralAssertionRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.VolatileLiteralAssertion;
    public override int Priority => 50;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.ConformanceOutcomeCode == ConformanceOutcomeCodes.ResponseSchemaViolation;
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context) => [];

    // islevi: Kararlilik olcumu bu taskta olmadigi icin tahmini Possible seviyesinde tutar.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => CreateAssessment(DiagnosisConfidenceCodes.Possible);
}
