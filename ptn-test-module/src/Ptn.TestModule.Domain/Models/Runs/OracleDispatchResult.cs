using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Uc hakemin adim hukumlerini, birlesmis bulgularini ve butceli teshis raporunu birlikte tasir.
// sistemdeki gorevi: Yargi asamasinin ciktisini terminal hukum cozumlemesine tek modelle verir.
/// <summary>
/// Bir kosumun oracle dagitim sonucunu tasir.
/// </summary>
public class OracleDispatchResult
{
    /// <summary>Kaynak sirasini koruyan adim hukumleridir.</summary>
    public IReadOnlyList<StepJudgement> Judgements { get; set; } = [];

    /// <summary>Uc hakemin bulgularinin kararli sirada birlestirilmis halidir.</summary>
    public IReadOnlyList<TestResultFindingModel> Findings { get; set; } = [];

    /// <summary>Satir ici butceye indirgenmis teshis raporu JSON metnidir.</summary>
    public string? DiagnosisReport { get; set; }
}
