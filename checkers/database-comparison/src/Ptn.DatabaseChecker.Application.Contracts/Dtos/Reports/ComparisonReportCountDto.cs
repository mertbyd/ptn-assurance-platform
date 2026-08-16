namespace Ptn.DatabaseChecker.Dtos.Reports;

// islevi: Rapor ozetinde tek bir kararli kodun (fark yonu ya da nesne turu) kac kez gectigini tasir (API modeli).
// sistemdeki gorevi: ComparisonReportCountModel'in cevap karsiligi; FE Code'u yuklu lookup listelerinden isme cozer (KindCounts/ObjectTypeCounts elemani).
public class ComparisonReportCountDto
{
    /// <summary>
    /// Sayilan kararli kod (DifferenceKindCodes.* veya SchemaObjectTypeCodes.* / Data / Migration).
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Bu kodun bulgular icindeki tekrar sayisi.
    /// </summary>
    public int Count { get; set; }
}
