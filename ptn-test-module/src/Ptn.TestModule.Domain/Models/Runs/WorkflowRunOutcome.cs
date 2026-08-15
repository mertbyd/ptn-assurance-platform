namespace Ptn.TestModule.Models.Runs;

// islevi: Runner surecinin cikis kodunu, HAR ile JSON artefaktini ve olculen suresini tasir.
// sistemdeki gorevi: Surec sinirinin gozlemini hukum vermeden yargi asamasina aktarir.
/// <summary>
/// Dis Arazzo runner surecinin tamamlanmis kosum ciktisini tasir.
/// </summary>
public class WorkflowRunOutcome
{
    /// <summary>Runner surecinin islenmemis cikis kodudur.</summary>
    public int ExitCode { get; set; }

    /// <summary>Runner'in urettigi HAR 1.2 belgesinin ham metnidir.</summary>
    public string HarContent { get; set; } = string.Empty;

    /// <summary>Runner'in urettigi butceli JSON ozet metnidir.</summary>
    public string JsonSummary { get; set; } = string.Empty;

    /// <summary>Surec sinirinda olculen toplam kosum suresidir.</summary>
    public long DurationMs { get; set; }

    /// <summary>Kosumu icra eden runner surumunun kararli referansidir.</summary>
    public string RunnerRef { get; set; } = string.Empty;
}
