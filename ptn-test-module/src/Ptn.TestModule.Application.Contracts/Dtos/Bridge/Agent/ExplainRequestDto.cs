namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_explain tool'unun kapali operasyon referansi ve outcome kodu girdisini tasir.
// sistemdeki gorevi: Teshis yuzeyini serbest operasyon, tablo, kolon veya scope metninden korur.
public sealed class ExplainRequestDto
{
    /// <summary>
    /// Kullanilacak profil paketinin kararli anahtarini belirtir.
    /// </summary>
    public string ProfileKey { get; set; } = string.Empty;
    /// <summary>
    /// Dogrulamada esas alinacak API sozlesme snapshot kimligini belirtir.
    /// </summary>
    public Guid SpecSnapshotId { get; set; }
    /// <summary>
    /// Checker isleminin calisacagi baglantinin kimligini belirtir.
    /// </summary>
    public Guid ConnectionId { get; set; }
    /// <summary>
    /// Cozumlenecek operasyonun kararli referans kimligini belirtir.
    /// </summary>
    public Guid OperationReferenceId { get; set; }
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string OutcomeCode { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public int? StatusCode { get; set; }
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
}
