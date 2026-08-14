using Ptn.TestModule.Dtos.Bridge.Diagnosis;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Aciklama agacindaki tek redaksiyonlu ve alintilanabilir kaniti tasir.
// sistemdeki gorevi: Kanitsiz aciklama dugumlerinin public rapora girmemesini sozlesmeyle gorunur kilar.
public sealed class EvidenceDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string ProbeKindCode { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string FactCode { get; set; } = string.Empty;
    /// <summary>
    /// Assertion tarafindaki beklenen veya gozlenen degeri belirtir.
    /// </summary>
    public string? ExpectedValue { get; set; }
    /// <summary>
    /// Assertion tarafindaki beklenen veya gozlenen degeri belirtir.
    /// </summary>
    public string? ObservedValue { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public long? ObservedAtMs { get; set; }
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public FindingRefDto? Ref { get; set; }
}
