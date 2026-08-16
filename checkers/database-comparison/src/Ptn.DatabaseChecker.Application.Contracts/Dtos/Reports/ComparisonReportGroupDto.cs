using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.Reports;

// islevi: Rapor detayinin tek bir grubu (bir nesne turu ya da bir tablo) + o grubun sayaclari (API modeli).
// sistemdeki gorevi: ComparisonReportGroupModel'in cevap karsiligi; "tur + tablo bazinda grupla" ciktisini tasir. Fark satirlarinin kendisi ComparisonReportDto.Findings'te durur; grup yalniz anahtar + sayac tutar.
public class ComparisonReportGroupDto
{
    /// <summary>
    /// Grubun anahtari: tur bazli grupta nesne turu kodu, tablo bazli grupta "sema.nesne".
    /// </summary>
    public string GroupKey { get; set; } = default!;

    /// <summary>
    /// Gruptaki toplam fark sayisi.
    /// </summary>
    public int DifferenceCount { get; set; }

    /// <summary>
    /// Grup icindeki yon (OnlyInSource/OnlyInTarget/Modified) dagilimi.
    /// </summary>
    public List<ComparisonReportCountDto> KindCounts { get; set; } = new();
}
