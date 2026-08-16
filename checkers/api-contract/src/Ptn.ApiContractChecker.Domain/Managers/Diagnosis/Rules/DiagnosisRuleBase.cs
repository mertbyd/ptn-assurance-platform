using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Rules;

// islevi: Hipotez assessment ve probe request kurulumunun ortak, kararsiz plumbing'ini toplar.
// sistemdeki gorevi: Her yeni hipotez sinifinin yalniz kendi uygulanabilirlik ve kanit kararini tasimasini saglar.
public abstract class DiagnosisRuleBase : IDiagnosisRule
{
    public abstract string HypothesisKindCode { get; }
    public abstract int Priority { get; }
    public abstract bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context);
    public abstract List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context);
    public abstract HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence);

    // islevi: Tek rule assessment'ini ortak kod, oncelik, guven ve sinirli kanitlarla kurar.
    protected HypothesisAssessment CreateAssessment(
        string confidenceCode,
        IEnumerable<ProbeEvidence>? evidence = null,
        SuggestedCheck? suggestedCheck = null)
        => new()
        {
            HypothesisKindCode = HypothesisKindCode,
            Priority = Priority,
            ConfidenceCode = confidenceCode,
            Evidence = evidence?.Where(item => item.HypothesisKindCode == HypothesisKindCode).ToList() ?? [],
            SuggestedCheck = suggestedCheck
        };

    // islevi: Fixed probe turunu mevcut rule ve context ile baglayan request'i kurar.
    protected ProbeRequest CreateProbe(
        string probeKindCode,
        ResolvedFailureContext context,
        string? factName = null,
        Uri? targetUri = null)
        => new()
        {
            ProbeKindCode = probeKindCode,
            HypothesisKindCode = HypothesisKindCode,
            FactName = factName,
            TargetUri = targetUri,
            AllowedServerUrls = context.Snapshot.Servers.ToList(),
            SpecPaths = context.Snapshot.Operations.Select(item => item.Path).Distinct().ToList(),
            Context = context
        };

    // islevi: Rule'a ait tek probe kanitini listeden bulur.
    protected ProbeEvidence? FindEvidence(List<ProbeEvidence> evidence, string probeKindCode)
        => evidence.FirstOrDefault(item => item.HypothesisKindCode == HypothesisKindCode &&
                                           item.ProbeKindCode == probeKindCode);
}
