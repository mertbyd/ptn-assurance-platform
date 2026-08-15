using Ptn.TestModule.Dtos.Bridge.Database;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_validate tool'unun snapshot, operasyon ve assertion referanslarini tasir.
// sistemdeki gorevi: Serbest JSON pointer veya operasyon adresi yerine kimlik tabanli yayin kapisi girdisi verir.
public sealed class ValidateRequestDto
{
    /// <summary>
    /// Kullanilacak profil paketinin kararli anahtarini belirtir.
    /// </summary>
    public string ProfileKey { get; set; } = string.Empty;
    /// <summary>
    /// Checker isleminin calisacagi baglantinin kimligini belirtir.
    /// </summary>
    public Guid ConnectionId { get; set; }
    /// <summary>
    /// Dogrulamada esas alinacak API sozlesme snapshot kimligini belirtir.
    /// </summary>
    public Guid SpecSnapshotId { get; set; }
    /// <summary>
    /// Cozumlenecek operasyonun kararli referans kimligini belirtir.
    /// </summary>
    public Guid OperationReferenceId { get; set; }
    /// <summary>
    /// Dogrulanacak assertion referans kimliklerini listeler.
    /// </summary>
    public List<Guid> AssertionReferenceIds { get; set; } = [];
    /// <summary>
    /// Dogrulama veya assertion girdilerini kararli sirada listeler.
    /// </summary>
    public List<DatabaseDerivabilityAddressDto> DatabaseAssertions { get; set; } = [];
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
}
