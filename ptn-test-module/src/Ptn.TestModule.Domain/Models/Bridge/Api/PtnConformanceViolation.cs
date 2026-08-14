namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek uygunluk ihlalinin kural, pointer ve schema keyword adresini tasir.
// sistemdeki gorevi: Hata degerini veya response govdesini rapora sizdirmadan konum kaniti verir.
public sealed class PtnConformanceViolation
{
    public string RuleCode { get; set; } = string.Empty;
    public string JsonPointer { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
}
