namespace Ptn.TestModule.Models.Runs;

// islevi: Runner surecinin yorumlanmamis cikis kodunu, artefakt metinlerini ve olculen suresini tasir.
// sistemdeki gorevi: Application surec sinirinin gozlemini, hukum vermeden Domain Manager'in yorumuna tasir.
/// <summary>
/// Dis runner surecinin ham gozlem sonucunu tasir.
/// </summary>
public class WorkflowRunProcessOutcome
{
    /// <summary>Runner surecinin islenmemis cikis kodudur.</summary>
    public int ExitCode { get; set; }

    /// <summary>Uretildiyse HAR artefaktinin ham metnidir; uretilmediyse null kalir.</summary>
    public string? HarContent { get; set; }

    /// <summary>Uretildiyse JSON ozetin ham metnidir.</summary>
    public string? JsonSummary { get; set; }

    /// <summary>Surec sinirinda olculen toplam kosum suresidir.</summary>
    public long DurationMs { get; set; }
}
