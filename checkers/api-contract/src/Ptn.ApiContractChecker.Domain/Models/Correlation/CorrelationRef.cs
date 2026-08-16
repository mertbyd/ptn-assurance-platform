namespace Ptn.ApiContractChecker.Models.Correlation;

// islevi: Cagiranin trace ve adim kimligini domain akisi boyunca yorumlamadan tasir.
// sistemdeki gorevi: Application.Contracts DTO'sunu Domain'e sizdirmadan manager-owned echo davranisini saglar.
public sealed class CorrelationRef
{
    public string? TraceId { get; set; }
    public string? StepKey { get; set; }
}
