namespace Ptn.DatabaseChecker.Models.Comparison.Reports;

// islevi: Rapor ozetinde tek bir kararli kodun (fark yonu ya da nesne turu) kac kez gectigini tasir.
// sistemdeki gorevi: "Kategoriler" ozetini besler; FE, Code'u yuklu lookup listelerinden isme cozer (KindCounts/ObjectTypeCounts elemani).
public class ComparisonReportCountModel
{
    // Sayilan kararli kod (DifferenceKindCodes.* veya SchemaObjectTypeCodes.*).
    public string Code { get; set; } = default!;

    // Bu kodun bulgular icindeki tekrar sayisi.
    public int Count { get; set; }
}
