using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Tek bir HAR adiminin hangi hakemden hangi hukmu aldigini ve urettigi bulgulari tasir.
// sistemdeki gorevi: Adim seviyesindeki hukmu kosum hukmunden ayirir; kayit sahipligini source_checker_code ile korur.
/// <summary>
/// Bir kosum adiminin hakem hukmunu ve bulgularini tasir.
/// </summary>
public class StepJudgement
{
    /// <summary>Hukmun baglandigi HAR entry'sidir.</summary>
    public HarEntryModel Entry { get; set; } = new();

    /// <summary>Hukmu veren hakemin kararli kaynak kodudur.</summary>
    public string SourceCheckerCode { get; set; } = string.Empty;

    /// <summary>Adim seviyesindeki Passed, Failed veya Inconclusive hukum kodudur.</summary>
    public string OutcomeCode { get; set; } = string.Empty;

    /// <summary>Hakemin bildirdigi ayrintili uygunluk veya kalicilik kodudur.</summary>
    public string CheckerOutcomeCode { get; set; } = string.Empty;

    /// <summary>Hukum olumsuzsa hangi hakemin hayir dedigini belirleyen kategori kodudur.</summary>
    public string? FailureCategoryCode { get; set; }

    /// <summary>Hukmun kararli ve makine-okur gerekce kodudur.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Bu adimin urettigi kalici bulgulardir.</summary>
    public IReadOnlyList<TestResultFindingModel> Findings { get; set; } = [];
}
