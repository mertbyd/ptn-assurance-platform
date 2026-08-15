using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Redaksiyonlu projeksiyon satirlarini ve kanit durumunu tasir.
// sistemdeki gorevi: Unavailable sonucunu yanlis yokluk hukumunden ayirir.
public sealed class ProjectionResultDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string StateCode { get; set; } = string.Empty;
    /// <summary>
    /// Butce dahilinde dondurulen projeksiyon satirlarini listeler.
    /// </summary>
    public List<Dictionary<string, string?>> Rows { get; set; } = [];
    /// <summary>
    /// Checker tarafindan gozlenen satir sayisini belirtir.
    /// </summary>
    public long ObservedRowCount { get; set; }
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool Truncated { get; set; }
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}
