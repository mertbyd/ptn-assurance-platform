namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Tek tablo lint uyarisinin kararli kodunu ve varsa ilgili generated kolonunu tasir.
// sistemdeki gorevi: Katalog siniflandirmasini Domain.Shared kodlariyla Mapperly cikisina hazirlar.
public sealed class SchemaLintWarningModel
{
    public string WarningCode { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
}
