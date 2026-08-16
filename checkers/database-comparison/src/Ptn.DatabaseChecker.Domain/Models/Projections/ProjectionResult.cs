using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Models.Projections;

// islevi: Projection outcome, redaksiyonlu satirlar, sayim, truncation ve correlation echo bilgisini tasir.
// sistemdeki gorevi: Repository ham okumasini public ama gizlilik-politikali domain sonucuna indirger.
public sealed class ProjectionResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<ProjectionRow> Rows { get; set; } = [];
    public int ObservedRowCount { get; set; }
    public bool Truncated { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
