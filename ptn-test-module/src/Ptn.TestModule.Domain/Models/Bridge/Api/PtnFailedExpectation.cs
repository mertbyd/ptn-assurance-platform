namespace Ptn.TestModule.Models.Bridge;

// islevi: Eslesmeyen tek kolon beklentisinin redaksiyonlu ozetini tasir.
// sistemdeki gorevi: Teshis kanitini hedef satirin ham degerlerini acmadan adresler.
public sealed class PtnFailedExpectation
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
}
