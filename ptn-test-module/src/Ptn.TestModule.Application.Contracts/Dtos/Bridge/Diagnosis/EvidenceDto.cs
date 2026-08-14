namespace Ptn.TestModule.Dtos.Bridge.Diagnosis;

// islevi: Tek teshis kanitinin olgu, deger, zaman ve kaynak referansini tasir.
// sistemdeki gorevi: Redaksiyonlu ve alintilanabilir kaniti public kontrata tasir.
public sealed class EvidenceDto
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string FactCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
    public long? ObservedAtMs { get; set; }
    public FindingRefDto? Ref { get; set; }
}
