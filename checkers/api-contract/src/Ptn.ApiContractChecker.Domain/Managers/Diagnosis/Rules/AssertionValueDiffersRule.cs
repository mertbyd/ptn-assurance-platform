using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Response schema violationini deger farki assertion hipotezi olarak aday yapar.
// sistemdeki gorevi: H-AS-01'i conformance outcome ve yapilandirilmis error code kanitindan uretir.
public sealed class AssertionValueDiffersRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.AssertionValueDiffers;
    public override int Priority => 55;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.ConformanceOutcomeCode == ConformanceOutcomeCodes.ResponseSchemaViolation;
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context) => [];

    // islevi: Acik value mismatch kodunda Likely, yalniz schema violation varsa Possible sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => CreateAssessment(identity.ProblemErrors.Any(item =>
                item.Code?.Contains(DiagnosisHttpConstants.ValueErrorToken, StringComparison.OrdinalIgnoreCase) == true)
            ? DiagnosisConfidenceCodes.Likely
            : DiagnosisConfidenceCodes.Possible);
}
