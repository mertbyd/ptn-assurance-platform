namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Tek probe'un kararli olgu kodu, gozlem zamani ve sinirli degerlerini tasir.
// sistemdeki gorevi: Hipotez degerlendirmesini aciklama metni ve ham response govdesinden ayirir.
public sealed class ProbeEvidence
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string HypothesisKindCode { get; set; } = string.Empty;
    public string FactCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
    public long? ObservedAtMs { get; set; }
}
