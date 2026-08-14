using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Normalize edilmis tek hipotezin oncelik, guven, kanit ve bulgu referansini tasir.
// sistemdeki gorevi: Alintisiz veya kaynaksiz hipotezin birlesik rapora girmesini engeller.
public sealed class PtnDiagnosisHypothesis
{
    public string HypothesisKindCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ConfidenceCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public PtnFindingRef Ref { get; set; } = new();
    public List<PtnEvidence> Evidence { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
}
