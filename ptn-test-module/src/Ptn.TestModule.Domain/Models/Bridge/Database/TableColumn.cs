namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek tablo kolonunun tip, nullability ve uretilme niteliklerini tasir.
// sistemdeki gorevi: Assertion adaylarini ham provider tiplerinden bagimsiz aciklar.
public sealed class TableColumn
{
    public string Name { get; set; } = string.Empty;
    public string CanonicalDataTypeCode { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
}
