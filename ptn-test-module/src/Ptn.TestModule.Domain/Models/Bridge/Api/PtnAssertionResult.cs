using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Database assertion outcome, sayim ve redaksiyonlu failure kanitini tasir.
// sistemdeki gorevi: Kalicilik hukum sonucunu ham satir degerlerini ajana acmadan domaine verir.
public sealed class PtnAssertionResult
{
    public string OutcomeCode { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public long ObservedRowCount { get; set; }
    public long ObservedAtMs { get; set; }
    public int AttemptCount { get; set; }
    public List<PtnFailedExpectation> FailedExpectations { get; set; } = [];
    public Dictionary<string, string?>? RowSummary { get; set; }

    // islevi: Eslesmeyen tek kolon beklentisinin redaksiyonlu ozetini tasir.
    // sistemdeki gorevi: Teshis kanitini hedef satirin ham degerlerini acmadan adresler.
    public sealed class PtnFailedExpectation
    {
        public string ColumnName { get; set; } = string.Empty;
        public string MatcherKindCode { get; set; } = string.Empty;
        public string? ExpectedValue { get; set; }
        public string? ObservedValue { get; set; }
    }
}
