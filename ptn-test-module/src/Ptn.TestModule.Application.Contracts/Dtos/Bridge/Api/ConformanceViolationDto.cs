namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tek response uygunluk ihlalinin kural ve konumunu tasir.
// sistemdeki gorevi: Checker ayrintisini kararli Bridge alanlarina indirger.
public sealed class ConformanceViolationDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string RuleCode { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string JsonPointer { get; set; } = string.Empty;
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Keyword { get; set; } = string.Empty;
}
