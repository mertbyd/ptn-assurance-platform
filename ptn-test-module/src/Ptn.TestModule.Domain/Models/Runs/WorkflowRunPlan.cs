using Ptn.TestModule.Models.Shared;

namespace Ptn.TestModule.Models.Runs;

// islevi: Kosumun surec planini, runner referansini ve artefakt yollarini birlikte tasir.
// sistemdeki gorevi: Surec sinirinin kosacagi plani, sonucu yorumlarken gereken kosum baglamiyla birlestirir.
/// <summary>
/// Dis runner cagrisinin kararli kosum planini tasir.
/// </summary>
public class WorkflowRunPlan
{
    /// <summary>Surec sinirinin oldugu gibi kosacagi tam cagri planidir.</summary>
    public ProcessExecutionPlan Process { get; set; } = new();

    /// <summary>Bu plani ureten runner surumunun kararli referansidir.</summary>
    public string RunnerRef { get; set; } = string.Empty;

    /// <summary>HAR artefaktinin calisma klasorune gore yoludur.</summary>
    public string HarFilePath { get; set; } = string.Empty;

    /// <summary>JSON ozet artefaktinin calisma klasorune gore yoludur.</summary>
    public string JsonFilePath { get; set; } = string.Empty;
}
