namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Normalize database assertion sonucunu tasir.
// sistemdeki gorevi: Redaksiyonlu sonucu public Bridge cevabi olarak sunar.
public sealed class AssertionResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public long ObservedRowCount { get; set; }
    public long ObservedAtMs { get; set; }
    public int AttemptCount { get; set; }
    public List<FailedExpectationDto> FailedExpectations { get; set; } = [];
    public Dictionary<string, string?>? RowSummary { get; set; }
}
