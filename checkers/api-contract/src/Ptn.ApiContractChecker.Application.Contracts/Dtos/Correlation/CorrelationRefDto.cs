namespace Ptn.ApiContractChecker.Dtos.Correlation;

// islevi: Cagiranin trace ve adim kimligini checker cagrisi boyunca tasir.
// sistemdeki gorevi: Sonucun hangi senaryo adimina ait oldugunu konumdan bagimsiz kilar.
public sealed class CorrelationRefDto
{
    public string? TraceId { get; set; }
    public string? StepKey { get; set; }
}
