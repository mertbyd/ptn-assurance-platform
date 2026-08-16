using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison.Reports;

// islevi: Raporun ozet blogu: toplam fark, kategori bazli sayaclar ve yon/nesne-turu dagilimlari.
// sistemdeki gorevi: "Ozet (kac fark, kategoriler)" ciktisini tasir; detay gruplarindan bagimsiz, tek bakista buyuk resmi verir.
public class ComparisonReportSummaryModel
{
    // Tum kategorilerdeki fark satirlarinin toplami.
    public int TotalDifferenceCount { get; set; }

    // Sema (yapi) fark sayisi.
    public int SchemaDifferenceCount { get; set; }

    // Veri (tablo/satir/hucre) fark sayisi.
    public int DataDifferenceCount { get; set; }

    // Migration defter fark sayisi.
    public int MigrationDifferenceCount { get; set; }

    // Yon bazli dagilim (OnlyInSource/OnlyInTarget/Modified basina kac fark).
    public List<ComparisonReportCountModel> KindCounts { get; set; } = new();

    // Nesne turu bazli dagilim (Table/Column/Index/View... basina kac fark); yalnizca sema farklari icin anlamlidir.
    public List<ComparisonReportCountModel> ObjectTypeCounts { get; set; } = new();
}
