using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek kolon matcher'inin kapali kod, beklenen deger ve tolerans alanlarini tasir.
// sistemdeki gorevi: Assertion beklentisini SQL veya provider ifadesi olmadan checker portuna verir.
public sealed class PtnColumnExpectation
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public List<string?> ExpectedValues { get; set; } = [];
    public decimal? Tolerance { get; set; }
}
