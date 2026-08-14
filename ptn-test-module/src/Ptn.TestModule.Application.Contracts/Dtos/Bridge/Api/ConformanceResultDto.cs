namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: HTTP response uygunluk hukumunu ve ihlallerini tasir.
// sistemdeki gorevi: Normalize checker sonucunu public Bridge cevabina tasir.
public sealed class ConformanceResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<ConformanceViolationDto> Violations { get; set; } = [];
}
