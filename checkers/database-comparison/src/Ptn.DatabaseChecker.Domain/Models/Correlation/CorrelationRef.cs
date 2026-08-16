namespace Ptn.DatabaseChecker.Models.Correlation;

// islevi: Cagiranin trace ve adim kimligini assertion ve teshis domain akislarinda kayipsiz tasir.
// sistemdeki gorevi: Application.Contracts DTO'sunu Domain'e sizdirmadan manager-owned echo davranisini mumkun kilar.
public sealed class CorrelationRef
{
    public string? TraceId { get; set; }
    public string? StepKey { get; set; }
}
