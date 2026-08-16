using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Dtos.Catalog;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_validate tool'unun kaynak belge, malzeme muhru ve geriye uyumlu kapali referanslarini tasir.
// sistemdeki gorevi: Mevcut derleme ve yayin gate sahiplerine serbest pointer uydurmadan kanitli girdi verir.
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
    /// Mevcut derleyici ve lint kapisinda denetlenecek Arazzo kaynak belgesini belirtir.
    /// </summary>
    public string? SourceDocument { get; set; }
    /// <summary>
    /// Yayin kapisinin denetleyecegi dort malzeme bagini tasir.
    /// </summary>
    public TestScenarioMaterialSealDto? MaterialSeal { get; set; }
    /// <summary>
    /// Dogrulama veya assertion girdilerini kararli sirada listeler.
    /// </summary>
    public List<DatabaseDerivabilityAddressDto> DatabaseAssertions { get; set; } = [];
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
}
