using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Assertions;

// islevi: Tek kolon icin matcher, beklenen degerler ve opsiyonel sayisal toleransi tasir.
// sistemdeki gorevi: Application DTO'su ile saf ValueMatcherEvaluator arasindaki persist edilmeyen domain deger nesnesidir.
public sealed class ColumnExpectation
{
    public string ColumnName { get; set; } = string.Empty;
    public string MatcherKindCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public List<string?> ExpectedValues { get; set; } = new();
    public decimal? Tolerance { get; set; }
}
