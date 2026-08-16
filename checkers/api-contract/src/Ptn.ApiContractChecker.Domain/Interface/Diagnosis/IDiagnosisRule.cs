using Ptn.ApiContractChecker.Models.Diagnosis;

namespace Ptn.ApiContractChecker.Interface.Diagnosis;

// islevi: Tek hipotezin olgu tabanli uygulanabilirlik, probe ihtiyaci ve assessment sozlesmesidir.
// sistemdeki gorevi: Yeni hipotezin DiagnosisManager veya mevcut rule dosyalarini degistirmeden DI koleksiyonuna katilmasini saglar.
public interface IDiagnosisRule
{
    string HypothesisKindCode { get; }
    int Priority { get; }
    bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context);
    List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context);
    HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence);
}
