using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Dogrulanmis auth challenge varken request kimligi gonderilmedi hipotezini degerlendirir.
// sistemdeki gorevi: H-AU-01'i status kodu eslemesi yerine challenge ve request header olgularina baglar.
public sealed class AuthenticationMissingRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.AuthenticationMissing;
    public override int Priority => 75;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.ChallengeScheme != null;
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context) => [];

    // islevi: Authorization header yoksa Confirmed, varsa RuledOut assessment uretir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => CreateAssessment(context.Signal.RequestHeaders.ContainsKey(DiagnosisHttpConstants.Authorization)
            ? DiagnosisConfidenceCodes.RuledOut
            : DiagnosisConfidenceCodes.Confirmed);
}
