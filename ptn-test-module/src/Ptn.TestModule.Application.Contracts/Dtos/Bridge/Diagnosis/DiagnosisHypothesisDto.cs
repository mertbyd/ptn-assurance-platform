namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: Tek teshis hipotezinin oncelik, guven, kanit ve sonraki kontrollerini tasir.
// sistemdeki gorevi: Kaynakli hipotezi public diagnosis kontratinda sunar.
public sealed class DiagnosisHypothesisDto
{
    public string HypothesisKindCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ConfidenceCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public FindingRefDto Ref { get; set; } = new();
    public List<EvidenceDto> Evidence { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
}
