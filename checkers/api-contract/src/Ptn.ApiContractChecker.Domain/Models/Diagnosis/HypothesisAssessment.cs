using Ptn.ApiContractChecker.Constants.Diagnosis;

namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Tek hipotezin kod, oncelik, guven, lokalize anlatim, kanit ve sonraki kontrollerini tasir.
// sistemdeki gorevi: Rule kararini deterministik siralama ile RFC 9457 raporundan ayri birimde tutar.
public sealed class HypothesisAssessment
{
    public string HypothesisKindCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ConfidenceCode { get; set; } = DiagnosisConfidenceCodes.Possible;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public List<ProbeEvidence> Evidence { get; set; } = new();
    public List<string> NextChecks { get; set; } = new();
    public SuggestedCheck? SuggestedCheck { get; set; }
}
