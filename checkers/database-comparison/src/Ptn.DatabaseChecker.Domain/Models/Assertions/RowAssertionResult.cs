using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Tek assertion'in kararli outcome, sayim, zaman, deneme ve hedefli failure kanitini tasir.
// sistemdeki gorevi: KBP-705 teshis motorunun girdisi ve Test Module senaryo adiminin kucuk cevap sozlesmesidir.
public sealed class RowAssertionResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public long ObservedRowCount { get; set; }
    public long ObservedAtMs { get; set; }
    public int AttemptCount { get; set; }
    public List<FailedExpectation> FailedExpectations { get; set; } = new();
    public Dictionary<string, string?>? RowSummary { get; set; }
    public CorrelationRef? Correlation { get; set; }
}
