namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kuru kosum celiskisini ajan ve HTTP tuketicisine kayipsiz acar.
// sistemdeki gorevi: Hukum degistirmeyen deterministik bildirimin public sozlesmesidir.
public sealed class DryRunContradictionReportDto
{
    public bool IsDryRun { get; set; }
    public bool HasContradiction { get; set; }
    public string Observation { get; set; } = string.Empty;
    public string Contract { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? OutcomeCode { get; set; }
}
