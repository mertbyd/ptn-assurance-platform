using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Yapilandirilmis required error kodunu zorunlu alan yok assertion hipotezine cevirir.
// sistemdeki gorevi: H-AS-02'yi problem detail metni okumadan RFC 9457 extension olgusundan uretir.
public sealed class AssertionRequiredFieldMissingRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.AssertionRequiredFieldMissing;
    public override int Priority => 57;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.ConformanceOutcomeCode == ConformanceOutcomeCodes.ResponseSchemaViolation ||
           identity.ProblemErrors.Count > 0;
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context) => [];

    // islevi: Required error kodu varsa Confirmed, yoksa RuledOut sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => CreateAssessment(identity.ProblemErrors.Any(item =>
                string.Equals(item.Code, DiagnosisHttpConstants.RequiredErrorCode, StringComparison.OrdinalIgnoreCase))
            ? DiagnosisConfidenceCodes.Confirmed
            : DiagnosisConfidenceCodes.RuledOut);
}
