using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison.Reports;

// islevi: Rapor detayinin tek bir grubu (bir nesne turu ya da bir tablo) + o grubun sayaclari; satir verisi TASIMAZ (detay bulgularda durur).
// sistemdeki gorevi: "Farklari tur + tablo bazinda grupla" ciktisini tasir; ObjectTypeGroups ve TableGroups ayni tipten beslenir (is bir kez yapilir). Fark satirlarinin kendisi ComparisonFindings'te oldugu icin burada yalniz gruplama anahtari + sayaclar tutulur (finding'i ikinci kez modellemez).
public class ComparisonReportGroupModel
{
    // Grubun anahtari: tur bazli grupta nesne turu kodu, tablo bazli grupta "sema.nesne".
    public string GroupKey { get; set; } = default!;

    // Gruptaki toplam fark sayisi.
    public int DifferenceCount { get; set; }

    // Grup icindeki yon (DifferenceKindCodes.*) dagilimi: OnlyInSource/OnlyInTarget/Modified basina kac fark.
    public List<ComparisonReportCountModel> KindCounts { get; set; } = new();
}
