namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek sema lint uyarisini public Bridge cevabinda tasir.
// sistemdeki gorevi: Ajanin tablo risklerini mesaj parse etmeden yorumlamasini saglar.
public sealed class SchemaLintWarningDto
{
    public string WarningCode { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
}
