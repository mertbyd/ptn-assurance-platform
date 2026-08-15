using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Bir Running kosumun degismez terminal sonucunu ve bulgularini tasir.
// sistemdeki gorevi: Run durum gecisi ile result aggregate yazimini tek Application UoW girdisinde birlestirir.
/// <summary>Test kosumunun terminal sonuc yazma girdisidir.</summary>
public sealed class WriteTestRunTerminalDto
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

    /// <summary>Kosumun milisaniye cinsinden suresidir.</summary>
    public int DurationMs { get; set; }

    /// <summary>ABP BLOB Storing icindeki opsiyonel HAR artefakt adidir.</summary>
    public string? HarBlobName { get; set; }

    /// <summary>Terminal sonucuyla atomik yazilacak bulgulardir.</summary>
    public List<TestResultFindingInputDto> Findings { get; set; } = [];
}
