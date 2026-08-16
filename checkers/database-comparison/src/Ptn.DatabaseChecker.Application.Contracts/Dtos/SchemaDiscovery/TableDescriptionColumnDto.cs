namespace Ptn.DatabaseChecker.Dtos.SchemaDiscovery;

// islevi: DescribeTable cevabinda tek kolonun ad, kanonik tip ve nullability ozetini tasir.
// sistemdeki gorevi: Senaryo yazari matcher tip semantigini ham provider tipinden bagimsiz secer.
public class TableDescriptionColumnDto
{
    public string Name { get; set; } = string.Empty;
    public string CanonicalDataTypeCode { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
}
