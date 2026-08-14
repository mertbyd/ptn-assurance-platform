namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Basarisiz tek kolon beklentisinin redaksiyonlu degerlerini tasir.
// sistemdeki gorevi: Ham veri tasimadan assertion farkini public kontrata verir.
public sealed class FailedExpectationDto
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
    public string? ObservedValue { get; set; }
}
