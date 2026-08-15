using System.Collections.Generic;
using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir kosum denemesinin ihracata giren hukum, sure ve bulgu alanlarini kod cozulmus halde tasir.
// sistemdeki gorevi: Lookup kimligini kararli hukum koduna cevrilmis olarak ihracat hesabina verir (PLAN-0003 TM-14/TM-30).
/// <summary>
/// Ihracat hesabina giren tek terminal denemesinin okuma modelidir.
/// </summary>
public class RunExportAttempt
{
    /// <summary>Ayni kosum icindeki bir tabanli deneme numarasidir.</summary>
    public int Attempt { get; set; }

    /// <summary>Denemenin kararli terminal hukum kodudur.</summary>
    public string OutcomeCode { get; set; } = string.Empty;

    /// <summary>Denemenin milisaniye cinsinden suresidir.</summary>
    public int DurationMs { get; set; }

    /// <summary>Kararli ve makine-okur hata kodudur.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Bu olusuma ozel RFC 9457 detail metnidir.</summary>
    public string? Detail { get; set; }

    /// <summary>Basarisiz adimin gorunen adidir.</summary>
    public string? FailedStepName { get; set; }

    /// <summary>Basarisiz adimin bir tabanli sira numarasidir.</summary>
    public int? FailedStepOrdinal { get; set; }

    /// <summary>Denemenin kararli sirali bulgularidir.</summary>
    public IReadOnlyList<TestResultFinding> Findings { get; set; } = [];
}
