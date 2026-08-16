using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.Assertions;

// islevi: Tek kolon assertion beklentisinin API girdi modelidir.
// sistemdeki gorevi: Matcher kodu, beklenen deger/listesi ve toleransi Test Module'dan Application katmanina tasir.
public class ColumnExpectationDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public List<string?> ExpectedValues { get; set; } = new();
    public decimal? Tolerance { get; set; }
}
