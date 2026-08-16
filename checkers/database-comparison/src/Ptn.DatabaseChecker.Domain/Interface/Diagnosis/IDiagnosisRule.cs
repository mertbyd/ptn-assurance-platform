using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Diagnosis;

namespace Ptn.DatabaseChecker.Interface.Diagnosis;

// islevi: Tek teshis hipotezinin olgu tabanli uygulanabilirlik, probe ihtiyaci ve degerlendirme sozlesmesidir.
// sistemdeki gorevi: Yeni hipotezin DiagnosisManager'a kod eklemeden conventional DI koleksiyonuna katilmasini saglar.
public interface IDiagnosisRule
{
    string HypothesisKindCode { get; }
    int Priority { get; }

    // islevi: Hata kodu esitligi yerine cikarilmis kimlik ve canli katalog olgulariyla kurali aday yapar.
    bool AppliesTo(FailureIdentity identity, ResolvedFailureContext context);

    // islevi: Hipotezin karar verebilmesi icin gereken sinirli salt-okuma probe isteklerini kurar.
    List<ProbeRequest> RequiredProbes(FailureIdentity identity, ResolvedFailureContext context);

    // islevi: Katalog olgulari ve tamamlanmis probe kanitlarini guven seviyesine cevirir.
    HypothesisAssessment Assess(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence);
}
