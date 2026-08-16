namespace Ptn.ApiContractChecker.Dtos.Diagnosis;

// islevi: Tek hipotezin kod, guven, anlatim, kanit ve sonraki kontrollerini tasir.
// sistemdeki gorevi: Deterministik rule sonucunu sirali RFC rapor uzantisi olarak disariya acar.
public sealed class HypothesisAssessmentDto
{
    public string HypothesisKindCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ConfidenceCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public List<ProbeEvidenceDto> Evidence { get; set; } = new();
    public List<string> NextChecks { get; set; } = new();
    public SuggestedCheckDto? SuggestedCheck { get; set; }
}
