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
    public Dictionary<string, List<string>> Facts { get; set; } = [];
    public List<PtnDiagnosisHypothesis> Hypotheses { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
}
