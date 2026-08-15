namespace Ptn.TestModule.Models.Runs;

// islevi: Kuru kosum gozlemi ile sozlesme hukmu arasindaki mekanik celiskiyi tasir.
// sistemdeki gorevi: Ajana yonlendirme vermeden gozlem, sozlesme ve celiski konumunu ayri alanlarda sunar.
public class DryRunContradictionReport
{
    public bool IsDryRun { get; set; }
    public bool HasContradiction { get; set; }
    public string Observation { get; set; } = string.Empty;
    public string Contract { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? OutcomeCode { get; set; }
}
