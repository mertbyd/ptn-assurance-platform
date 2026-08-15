namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Cagiranin W3C trace ve senaryo adim kimligini public Bridge kontratinda tasir.
// sistemdeki gorevi: Checker request ve response'larini liste konumundan bagimsiz eslestirir.
public sealed class CorrelationRefDto
{
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string? TraceId { get; set; }
    /// <summary>
    /// Senaryo adiminin kararli korelasyon anahtarini belirtir.
    /// </summary>
    public string? StepKey { get; set; }
}
