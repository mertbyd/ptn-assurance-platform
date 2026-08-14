namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Basarisiz tek kolon beklentisinin redaksiyonlu degerlerini tasir.
// sistemdeki gorevi: Ham veri tasimadan assertion farkini public kontrata verir.
public sealed class FailedExpectationDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
}
