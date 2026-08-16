using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Models.Comparison.Findings;

// islevi: Kapsamda DataCompare isaretli bir tablonun veri karsilastirma ozeti (tablo basina bir eleman); kademeli mantik: COUNT -> HASH -> satir detayi.
// sistemdeki gorevi: Eski DataDifference tablosunun owned-JSON karsiligi; satir/hucre farklarini ic ice (RowDifferences) tasir - eskiden 3 ayri tabloydu, simdi tek nested nesne.
public class DataDifferenceModel
{
    /// <summary>
    /// Ayni veri farkini kosular arasinda kararli olarak tanimlayan SHA-256 degeri.
    /// </summary>
    public string Fingerprint { get; set; } = default!;

    /// <summary>
    /// Veri farkinin uyumluluk etkisini belirleyen kararli siddet kodu.
    /// </summary>
    public string SeverityCode { get; set; } = default!;

    // Karsilastirilan tablonun semasi (adres: muhasebe.faturalar).
    public string SchemaName { get; set; } = default!;

    // Karsilastirilan tablonun adi.
    public string TableName { get; set; } = default!;

    // Row-count farkinin yon kodu (DifferenceKindCodes.*): kaynakta fazla, hedefte fazla veya daha derin veri farki.
    public string KindCode { get; set; } = default!;

    // Kaynaktaki COUNT(*); null = sayim alinamadi (timeout/yetki) - "0 satir" ile "sayamadim" asla karismaz.
    public long? SourceRowCount { get; set; }

    // Hedefteki COUNT(*); null = sayim alinamadi.
    public long? TargetRowCount { get; set; }

    // Kaynak eksi hedef signed satir farki; null = taraflardan en az biri sayilamadi.
    public long? RowCountDifference { get; set; }

    // Kaynak icerik parmak izi; kademe 2'ye hic gecilmediyse (sayilar zaten farkliysa) null kalir.
    public string? SourceHash { get; set; }

    // Hedef icerik parmak izi.
    public string? TargetHash { get; set; }

    // Kademe 3'e inildi mi; true = satir detayi cikarildi (RowDifferences dolu).
    public bool HasRowLevelDifference { get; set; }

    // Fark bulunan satirlar (kademe 3); PK bazli eslesme + yon. Iç içe hucre farklarini tasir.
    public List<DataRowDifferenceModel> RowDifferences { get; set; } = new();
}
