namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek database kolon beklentisini tasir.
// sistemdeki gorevi: Matcher girdisini serbest SQL olmadan public kontrata tasir.
public sealed class ColumnExpectationDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public List<string?> ExpectedValues { get; set; } = [];
    public decimal? Tolerance { get; set; }
}
