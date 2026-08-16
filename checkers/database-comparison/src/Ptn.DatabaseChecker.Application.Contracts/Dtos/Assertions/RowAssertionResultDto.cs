using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Correlation;

namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Tek assertion'in outcome, sayim, zaman, deneme ve failure kanitini API cevabinda tasir.
// sistemdeki gorevi: Test Module senaryo adimi ve KBP-705 teshis girdisi icin kararli, kucuk ve secret icermeyen sonuc sozlesmesidir.
public class RowAssertionResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public long ObservedRowCount { get; set; }
    public long ObservedAtMs { get; set; }
    public int AttemptCount { get; set; }
    public List<FailedExpectationDto> FailedExpectations { get; set; } = new();
    public Dictionary<string, string?>? RowSummary { get; set; }
    public CorrelationRefDto? Correlation { get; set; }
}
