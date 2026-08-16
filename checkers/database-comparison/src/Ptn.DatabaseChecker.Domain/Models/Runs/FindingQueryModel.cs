namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Kalici run bulgulari icin filtre ve sinirlandirilmis sayfalama parametrelerini tasir.
// sistemdeki gorevi: Application DTO'sunu Domain/Repository katmanlarina tasimadan MCP bulgu sorgusunu ifade eder.
/// <summary>
/// Kalici run bulgulari icin filtre ve sayfalama parametreleri.
/// </summary>
public sealed class FindingQueryModel
{
    /// <summary>Opsiyonel siddet kodu filtresi.</summary>
    public string? SeverityCode { get; set; }
    /// <summary>Opsiyonel fark yonu kodu filtresi.</summary>
    public string? KindCode { get; set; }
    /// <summary>Opsiyonel sema nesne turu kodu filtresi.</summary>
    public string? ObjectTypeCode { get; set; }
    /// <summary>Opsiyonel sema adi filtresi.</summary>
    public string? SchemaName { get; set; }
    /// <summary>Opsiyonel tablo adi filtresi.</summary>
    public string? TableName { get; set; }
    /// <summary>Ayni definition icindeki tamamlanmis referans run kimligi.</summary>
    public Guid? SinceRunId { get; set; }
    /// <summary>Opsiyonel ve normalize edilmis fingerprint altkumesi.</summary>
    public List<string> Fingerprints { get; set; } = [];
    /// <summary>Atlanacak filtreli bulgu sayisi.</summary>
    public int SkipCount { get; set; }
    /// <summary>Istenen azami sonuc sayisi.</summary>
    public int MaxResultCount { get; set; }
}
