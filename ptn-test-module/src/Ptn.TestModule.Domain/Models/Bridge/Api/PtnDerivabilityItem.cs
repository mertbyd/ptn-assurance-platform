namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek assertion pointer'i ve kapali outcome kodunu tasir.
// sistemdeki gorevi: Turetilemeyen assertion'i dogrudan kaynak konumuyla raporlar.
public sealed class PtnDerivabilityItem
{
    public string JsonPointer { get; set; } = string.Empty;
    public string OutcomeCode { get; set; } = string.Empty;
}
