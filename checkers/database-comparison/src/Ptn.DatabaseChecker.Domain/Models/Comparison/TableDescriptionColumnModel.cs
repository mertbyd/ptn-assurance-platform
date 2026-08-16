namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Senaryo yazimi icin bir tablo kolonunun ad, kanonik tip ve nullability ozetini tasir.
// sistemdeki gorevi: Tam schema snapshot'in kucuk DescribeTable cevabina indirgenmis kolon modelidir.
public sealed class TableDescriptionColumnModel
{
    public string Name { get; set; } = string.Empty;
    public string CanonicalDataTypeCode { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
}
