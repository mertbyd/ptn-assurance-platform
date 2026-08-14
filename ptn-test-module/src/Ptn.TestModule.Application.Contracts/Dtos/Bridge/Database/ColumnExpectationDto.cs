namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek database kolon beklentisini tasir.
// sistemdeki gorevi: Matcher girdisini serbest SQL olmadan public kontrata tasir.
public sealed class ColumnExpectationDto
{
    /// <summary>
    /// Hedef kolonun adini veya kararli adresini belirtir.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string MatcherKindCode { get; set; } = string.Empty;
    /// <summary>
    /// Assertion tarafindaki beklenen veya gozlenen degeri belirtir.
    /// </summary>
    public string? ExpectedValue { get; set; }
    /// <summary>
    /// Assertion tarafindaki beklenen veya gozlenen degeri belirtir.
    /// </summary>
    public List<string?> ExpectedValues { get; set; } = [];
    /// <summary>
    /// Karar veya eslesme icin kullanilan sayisal olcuyu belirtir.
    /// </summary>
    public decimal? Tolerance { get; set; }
}
