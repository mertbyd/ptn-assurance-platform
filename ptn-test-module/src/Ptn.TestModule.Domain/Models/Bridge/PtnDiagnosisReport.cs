using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Iki checker teshis raporunun ortak RFC ve kanitli hipotez seklini tasir.
// sistemdeki gorevi: Kaynak checker'i koruyarak hipotez gramerini tek kopru sozlugune cevirir.
public sealed class PtnDiagnosisReport
{
    public string SourceCheckerCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public PtnLocation Location { get; set; } = new();
    public List<PtnDiagnosisHypothesis> Hypotheses { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];

    // islevi: Normalize edilmis tek hipotezin oncelik, guven, kanit ve bulgu referansini tasir.
    // sistemdeki gorevi: Alintisiz veya kaynaksiz hipotezin birlesik rapora girmesini engeller.
    public sealed class PtnDiagnosisHypothesis
    {
        public string HypothesisCode { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ConfidenceCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public PtnFindingRef Ref { get; set; } = new();
        public List<PtnEvidence> Evidence { get; set; } = [];
        public List<string> NextChecks { get; set; } = [];
    }
}
