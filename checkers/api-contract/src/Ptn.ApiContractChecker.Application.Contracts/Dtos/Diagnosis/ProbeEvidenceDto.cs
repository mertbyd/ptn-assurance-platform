namespace Ptn.ApiContractChecker.Dtos.Diagnosis;

// islevi: Tek hipoteze ait yapilandirilmis ve sinirli probe kanitini tasir.
// sistemdeki gorevi: Rapor kanitini ham response body veya aciklama metninden bagimsiz tutar.
public sealed class ProbeEvidenceDto
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string HypothesisKindCode { get; set; } = string.Empty;
    public string FactCode { get; set; } = string.Empty;
    public string? ExpectedValue { get; set; }
    public string? ObservedValue { get; set; }
    public long? ObservedAtMs { get; set; }
}
