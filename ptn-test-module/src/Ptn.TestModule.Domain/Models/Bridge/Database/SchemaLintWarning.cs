namespace Ptn.TestModule.Models.Bridge;

// islevi: Tablo assertion yazarligini etkileyen tek sema lint uyarisini tasir.
// sistemdeki gorevi: Yayin kapisina kararli kodu ve opsiyonel kolon kanitini provider-bagimsiz verir.
public sealed class SchemaLintWarning
{
    public string WarningCode { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
}
