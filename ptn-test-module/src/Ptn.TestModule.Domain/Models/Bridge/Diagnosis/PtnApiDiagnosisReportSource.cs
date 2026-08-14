using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Diagnosis;

// islevi: API checker diagnosis raporunun normalize edilmemis kaynak seklini tasir.
// sistemdeki gorevi: Checker DTO'sunu Domain Manager kararlarindan ayiran Mapperly hedefidir.
public sealed class PtnApiDiagnosisReportSource
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public PtnApiFailureIdentity Identity { get; set; } = new();
    public PtnApiDiagnosisLocation Location { get; set; } = new();
    public List<PtnApiDiagnosisHypothesis> Hypotheses { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
}
