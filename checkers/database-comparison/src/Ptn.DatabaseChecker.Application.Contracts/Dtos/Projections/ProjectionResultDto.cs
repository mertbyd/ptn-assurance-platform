using Ptn.DatabaseChecker.Dtos.Correlation;

namespace Ptn.DatabaseChecker.Dtos.Projections;

// islevi: Projection outcome, redaksiyonlu satirlar, sayim, truncation ve correlation echo bilgisini HTTP cevabinda tasir.
// sistemdeki gorevi: Tuketiciye ham hedef verisi veya sessiz kesme olmadan sinirli okuma kaniti verir.
public sealed class ProjectionResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<ProjectionRowDto> Rows { get; set; } = [];
    public int ObservedRowCount { get; set; }
    public bool Truncated { get; set; }
    public CorrelationRefDto? Correlation { get; set; }
}
