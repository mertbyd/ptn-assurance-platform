using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: HTTP response uygunluk hukumunu ve ihlallerini tasir.
// sistemdeki gorevi: Normalize checker sonucunu public Bridge cevabina tasir.
public sealed class ConformanceResultDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string OutcomeCode { get; set; } = string.Empty;
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<ConformanceViolationDto> Violations { get; set; } = [];
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}
