using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Dtos.Findings;

// islevi: Bir tablonun veri karsilastirma ozetinin API modelidir.
// sistemdeki gorevi: DataDifferenceModel'in cevap karsiligi; satir/hucre farklarini ic ice (RowDifferences) tasir.
public class DataDifferenceDto
{
    /// <summary>
    /// Kosular arasinda kararli bulgu parmak izi.
    /// </summary>
    public string Fingerprint { get; set; } = default!;

    /// <summary>
    /// Veri farkinin kararli siddet kodu.
    /// </summary>
    public string SeverityCode { get; set; } = default!;

    /// <summary>
    /// Karsilastirilan tablonun semasi.
    /// </summary>
    public string SchemaName { get; set; } = default!;

    /// <summary>
    /// Karsilastirilan tablonun adi.
    /// </summary>
    public string TableName { get; set; } = default!;

    /// <summary>
    /// Row-count farkinin yon kodu (DifferenceKindCodes.*).
    /// </summary>
    public string KindCode { get; set; } = default!;

    /// <summary>
    /// Kaynaktaki COUNT(*); null = sayim alinamadi (0'dan farkli anlam).
    /// </summary>
    public long? SourceRowCount { get; set; }

    /// <summary>
    /// Hedefteki COUNT(*); null = sayim alinamadi.
    /// </summary>
    public long? TargetRowCount { get; set; }

    /// <summary>
    /// Kaynak eksi hedef signed satir farki; null = taraflardan en az biri sayilamadi.
    /// </summary>
    public long? RowCountDifference { get; set; }

    /// <summary>
    /// Kaynak icerik parmak izi; sayilar zaten farkliysa null.
    /// </summary>
    public string? SourceHash { get; set; }

    /// <summary>
    /// Hedef icerik parmak izi.
    /// </summary>
    public string? TargetHash { get; set; }

    /// <summary>
    /// Satir detayina inildi mi (RowDifferences dolu mu).
    /// </summary>
    public bool HasRowLevelDifference { get; set; }

    /// <summary>
    /// Fark bulunan satirlar; PK bazli eslesme + yon.
    /// </summary>
    public List<DataRowDifferenceDto> RowDifferences { get; set; } = new();
}
