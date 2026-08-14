using Ptn.TestModule.Dtos.Bridge.Diagnosis;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Aciklama agacindaki tek redaksiyonlu ve alintilanabilir kaniti tasir.
// sistemdeki gorevi: Kanitsiz aciklama dugumlerinin public rapora girmemesini sozlesmeyle gorunur kilar.
public sealed class PtnEvidenceDto
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string FactCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
    public long? ObservedAtMs { get; set; }
    public FindingRefDto? Ref { get; set; }
}
