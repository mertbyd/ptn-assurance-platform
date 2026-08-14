using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Tek bir kosum denemesinin hukum, sorun, teshis ve bulgu girdilerini tasir.
// sistemdeki gorevi: Terminal yazimin tamamini TestRunResultManager'a tek domain modeliyle verir.
/// <summary>
/// Test kosumunun terminal sonucunu ve bulgularini tasir.
/// </summary>
public class TestRunTerminalModel
{
    /// <summary>Passed, Failed, Broken, Skipped veya Inconclusive hukum kodudur.</summary>
    public string OutcomeCode { get; set; } = string.Empty;

    /// <summary>Hangi hakemin hayir dedigini belirleyen opsiyonel lookup kodudur.</summary>
    public string? FailureCategoryCode { get; set; }

    /// <summary>Kararli ve makine-okur hata kodudur.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Bu olusuma ozel RFC 9457 detail metnidir.</summary>
    public string? Detail { get; set; }

    /// <summary>Basarisiz adimin bir tabanli sira numarasidir.</summary>
    public int? FailedStepOrdinal { get; set; }

    /// <summary>Basarisiz adimin gorunen adidir.</summary>
    public string? FailedStepName { get; set; }

    /// <summary>Basarisiz adimin makine-okur yoludur.</summary>
    public string? FailedStepPath { get; set; }

    /// <summary>Kosumun terminale giderken izledigi dal yoludur.</summary>
    public string? TakenBranchPath { get; set; }

    /// <summary>Basariyla tamamlanan son adimin bir tabanli sira numarasidir.</summary>
    public int? LastCompletedOrdinal { get; set; }

    /// <summary>En fazla 4 KB olan yapilandirilmis diagnosis JSON metnidir.</summary>
    public string? DiagnosisReport { get; set; }

    /// <summary>Terminal sonucla atomik yazilacak bulgulardir.</summary>
    public IReadOnlyCollection<TestResultFindingModel> Findings { get; set; } = [];
}
