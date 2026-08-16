using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.Reports;

// islevi: Raporun ozet blogu: toplam fark, kategori bazli sayaclar ve yon/nesne-turu dagilimlari (API modeli).
// sistemdeki gorevi: ComparisonReportSummaryModel'in cevap karsiligi; "Ozet (kac fark, kategoriler)" ciktisini tasir.
public class ComparisonReportSummaryDto
{
    /// <summary>
    /// Tum kategorilerdeki fark satirlarinin toplami.
    /// </summary>
    public int TotalDifferenceCount { get; set; }

    /// <summary>
    /// Sema (yapi) fark sayisi.
    /// </summary>
    public int SchemaDifferenceCount { get; set; }

    /// <summary>
    /// Veri (tablo/satir/hucre) fark sayisi.
    /// </summary>
    public int DataDifferenceCount { get; set; }

    /// <summary>
    /// Migration defter fark sayisi.
    /// </summary>
    public int MigrationDifferenceCount { get; set; }

    /// <summary>
    /// Yon bazli dagilim (OnlyInSource/OnlyInTarget/Modified basina kac fark).
    /// </summary>
    public List<ComparisonReportCountDto> KindCounts { get; set; } = new();

    /// <summary>
    /// Nesne turu bazli dagilim (Table/Column/Index/View... + Data/Migration basina kac fark).
    /// </summary>
    public List<ComparisonReportCountDto> ObjectTypeCounts { get; set; } = new();
}
