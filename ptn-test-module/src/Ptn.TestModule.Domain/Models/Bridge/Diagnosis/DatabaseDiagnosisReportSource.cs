using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Diagnosis;

// islevi: Database checker diagnosis raporunun normalize edilmemis kaynak seklini tasir.
// sistemdeki gorevi: Checker DTO'sunu Domain Manager kararlarindan ayiran Mapperly hedefidir.
public sealed class DatabaseDiagnosisReportSource
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public DatabaseDiagnosisLocation Location { get; set; } = new();
    public List<DatabaseDiagnosisHypothesis> Hypotheses { get; set; } = [];
    public List<string> NextChecks { get; set; } = [];
    public CorrelationRef? Correlation { get; set; }
}
