namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: Iki checker teshisinin ortak RFC, konum ve hipotez sonucunu tasir.
// sistemdeki gorevi: Normalize teshis kontratini Application.Contracts katmanindan sunar.
public sealed class DiagnosisReportDto
{
    public string SourceCheckerCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public LocationDto Location { get; set; } = new();
    public Dictionary<string, List<string>> Facts { get; set; } = [];
    public List<DiagnosisHypothesisDto> Hypotheses { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
}
