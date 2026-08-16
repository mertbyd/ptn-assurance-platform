namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Composition hostun cozecegi capability, operation ve parametre cantasini tasir.
// sistemdeki gorevi: API teshisini baska checker paketlerine compile-time bagimlilik kurmadan zincirler.
public sealed class SuggestedCheck
{
    public string CapabilityCode { get; set; } = string.Empty;
    public string OperationCode { get; set; } = string.Empty;
    public Dictionary<string, string?> Arguments { get; set; } = new(StringComparer.Ordinal);
}
