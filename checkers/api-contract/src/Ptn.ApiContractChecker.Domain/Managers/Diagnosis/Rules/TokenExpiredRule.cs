using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: RFC 6750 invalid_token challenge olgusunu token suresi dolmus hipotezine cevirir.
// sistemdeki gorevi: H-AU-02'yi challenge aciklamasi veya token govdesi okumadan degerlendirir.
public sealed class TokenExpiredRule : DiagnosisRuleBase, ITransientDependency
{
    public override string HypothesisKindCode => HypothesisKindCodes.TokenExpired;
    public override int Priority => 74;
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.ChallengeScheme != null;
    public override List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context) => [];

    // islevi: invalid_token kodunda Confirmed, diger challenge hatalarinda RuledOut sonucunu verir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => CreateAssessment(identity.ChallengeError == DiagnosisHttpConstants.InvalidToken
            ? DiagnosisConfidenceCodes.Confirmed
            : DiagnosisConfidenceCodes.RuledOut);
}
