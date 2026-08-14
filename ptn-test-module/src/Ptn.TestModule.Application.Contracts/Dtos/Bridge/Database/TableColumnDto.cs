namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek tablo kolonunun kanonik tip ve nullability bilgisini tasir.
// sistemdeki gorevi: Provider tipini kararli Bridge sozlugune indirger.
public sealed class TableColumnDto
{
    public string Name { get; set; } = string.Empty;
    public string CanonicalDataTypeCode { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
}
