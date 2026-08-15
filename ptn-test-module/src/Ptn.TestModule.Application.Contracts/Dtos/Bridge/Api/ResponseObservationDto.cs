using System.Text.Json;
using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Sozlesmeye karsi denetlenecek HTTP yanit gozlemini tasir.
// sistemdeki gorevi: Public response assertion girdisini tipli ve govde-sinirli tutar.
public sealed class ResponseObservationDto
{
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public Guid SnapshotId { get; set; }
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string? OperationId { get; set; }
    /// <summary>
    /// HTTP operasyonunun yontemini belirtir.
    /// </summary>
    public string Method { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public int StatusCode { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string? ContentType { get; set; }
    /// <summary>
    /// Istek veya provider tarafindaki ad-deger alanlarini belirtir.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public JsonElement? Body { get; set; }
    /// <summary>
    /// Senaryonun Strict, Runtime veya Lenient uygunluk profilini belirtir; bos birakilirsa Runtime kullanilir.
    /// </summary>
    public string? ProfileCode { get; set; }
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}
