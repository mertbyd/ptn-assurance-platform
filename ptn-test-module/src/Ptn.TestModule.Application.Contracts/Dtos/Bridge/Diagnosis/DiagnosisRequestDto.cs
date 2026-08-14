using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: API veya database teshis sinyalini ortak public girdide tasir.
// sistemdeki gorevi: Checker DTO ailelerini Test Module Application.Contracts sinirinin disinda tutar.
public sealed class DiagnosisRequestDto
{
    /// <summary>
    /// Dogrulamada esas alinacak API sozlesme snapshot kimligini belirtir.
    /// </summary>
    public Guid? SpecSnapshotId { get; set; }
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public Guid? ApiRunId { get; set; }
    /// <summary>
    /// Checker isleminin calisacagi baglantinin kimligini belirtir.
    /// </summary>
    public Guid ConnectionId { get; set; }
    /// <summary>
    /// Bulgunun kaynak icindeki konumunu tasir.
    /// </summary>
    public LocationDto Location { get; set; } = new();
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public int? StatusCode { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string? ContentType { get; set; }
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string? OutcomeCode { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string? TransportErrorCode { get; set; }
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string? EngineCode { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string? SqlState { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public long? ObservedAtMs { get; set; }
    /// <summary>
    /// Satiri adresleyen kolon-deger eslesmelerini belirtir.
    /// </summary>
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Dogrulama veya assertion girdilerini kararli sirada listeler.
    /// </summary>
    public List<FailedExpectationDto> FailedExpectations { get; set; } = [];
    /// <summary>
    /// Istek veya provider tarafindaki ad-deger alanlarini belirtir.
    /// </summary>
    public Dictionary<string, string?> ProviderFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}
