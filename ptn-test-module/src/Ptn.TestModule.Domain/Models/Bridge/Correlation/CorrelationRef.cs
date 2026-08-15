namespace Ptn.TestModule.Models.Bridge;

// islevi: Checker cagrilarinin trace ve adim korelasyonunu domain sinirinda tasir.
// sistemdeki gorevi: Iki checker DTO ailesini tek Bridge korelasyon modeliyle hizalar.
public sealed class CorrelationRef
{
    public string? TraceId { get; set; }
    public string? StepKey { get; set; }
}
