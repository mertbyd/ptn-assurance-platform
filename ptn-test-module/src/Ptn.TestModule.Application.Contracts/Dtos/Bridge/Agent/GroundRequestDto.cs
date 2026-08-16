using Ptn.TestModule.Dtos.Authoring;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_ground tool'unun profil, snapshot, baglanti, kapali referans ve oturum girdilerini tasir.
// sistemdeki gorevi: StepIntent disinda serbest operasyon, tablo, kolon, kod veya scope metni tasimaz.
public sealed class GroundRequestDto
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
    public Guid? OperationReferenceId { get; set; }
    /// <summary>
    /// Hedef tablonun varsa kararli referans kimligini belirtir.
    /// </summary>
    public Guid? TableReferenceId { get; set; }
    /// <summary>
    /// Senaryo adiminin kanonik amac kodunu belirtir.
    /// </summary>
    public string StepIntent { get; set; } = string.Empty;
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool HasExclusiveSandbox { get; set; }
    /// <summary>
    /// Surdurulecek yazarlik oturumunun kimligini belirtir; bos birakilirsa oturum okunmaz.
    /// </summary>
    public Guid? SessionId { get; set; }
    /// <summary>
    /// Oturuma eklenecek tek yapilandirilmis adim onerisini tasir.
    /// </summary>
    public AddAuthoringStepDto? ProposedStep { get; set; }
}
