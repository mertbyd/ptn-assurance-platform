using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Sozlesme sapmasi hipotezlerini RelatedFindings kind kodu ve fact probe kanitiyla degerlendirir.
// sistemdeki gorevi: Yedi contract-drift rule sinifinda ayni aday, probe ve guven akisini tekrar etmez.
public abstract class FindingDiagnosisRuleBase : DiagnosisRuleBase
{
    protected abstract IReadOnlyCollection<string> FindingKindCodes { get; }
    public override int Priority => 90;

    // islevi: Hata sinyali veya ilgili finding varsa sozlesme sapmasini aday yapar.
    public override bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context)
        => identity.StatusClassCode is HttpStatusClassCodes.ClientError or HttpStatusClassCodes.ServerError ||
           !string.IsNullOrWhiteSpace(identity.ConformanceOutcomeCode) ||
           context.RelatedFindings.Any(item => FindingKindCodes.Contains(item.KindCode));

    // islevi: Her kabul edilen difference kind icin agsiz ContractDriftFact probe istegi uretir.
    public override List<ProbeRequest> RequiredProbes(
        FailureIdentity identity,
        ResolvedFailureContext context)
        => FindingKindCodes.Select(kind => CreateProbe(ProbeKindCodes.ContractDriftFact, context, kind)).ToList();

    // islevi: Matching finding varsa Confirmed, yoksa RuledOut assessment uretir.
    public override HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
    {
        var proofs = evidence.Where(item => item.HypothesisKindCode == HypothesisKindCode).ToList();
        var confirmed = proofs.Any(item => item.FactCode == ProbeKindCodes.Facts.Present);
        return CreateAssessment(confirmed ? DiagnosisConfidenceCodes.Confirmed : DiagnosisConfidenceCodes.RuledOut,
            proofs);
    }
}
