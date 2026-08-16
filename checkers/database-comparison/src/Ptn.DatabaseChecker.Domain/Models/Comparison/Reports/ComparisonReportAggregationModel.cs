using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison.Reports;

// islevi: Bir run'in bulgularindan hesaplanan raporun SADECE aggregation kismini tasir: ozet + tur/tablo bazli gruplama.
// sistemdeki gorevi: ComparisonReportBuilder'in ciktisi; header/detay (baglanti adlari, findings) buraya GIRMEZ - onlar Mapperly ile entity'den DTO'ya duzlestirilir. Boylece builder yalnizca Mapperly'nin yapamadigi sayim/gruplama isini yapar (elle header kopyalamaz).
public class ComparisonReportAggregationModel
{
    // Ozet blogu: toplam/kategori sayaclari + yon/nesne-turu dagilimi.
    public ComparisonReportSummaryModel Summary { get; set; } = new();

    // Gruplama: farklarin nesne turu bazinda sayac dagilimi.
    public List<ComparisonReportGroupModel> ObjectTypeGroups { get; set; } = new();

    // Gruplama: farklarin tablo (sema.nesne) bazinda sayac dagilimi.
    public List<ComparisonReportGroupModel> TableGroups { get; set; } = new();
}
