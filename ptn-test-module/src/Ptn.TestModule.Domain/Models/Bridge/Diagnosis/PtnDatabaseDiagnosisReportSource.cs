using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Diagnosis;

// islevi: Database checker diagnosis raporunun normalize edilmemis kaynak seklini tasir.
// sistemdeki gorevi: Checker DTO'sunu Domain Manager kararlarindan ayiran Mapperly hedefidir.
public sealed class PtnDatabaseDiagnosisReportSource
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public PtnDatabaseDiagnosisLocation Location { get; set; } = new();
    public List<PtnDatabaseDiagnosisHypothesis> Hypotheses { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
}
