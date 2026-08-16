namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Tek hipotezin istedigi olgu veya safe HTTP probe turu ile dogrulanabilir hedefini tasir.
// sistemdeki gorevi: Kurallar ile probe implementasyonlarini serbest URL ve unsafe method yetenegi vermeden baglar.
public sealed class ProbeRequest
{
    public string ProbeKindCode { get; set; } = string.Empty;
    public string HypothesisKindCode { get; set; } = string.Empty;
    public Uri? TargetUri { get; set; }
    public List<string> AllowedServerUrls { get; set; } = new();
    public List<string> SpecPaths { get; set; } = new();
    public string? FactName { get; set; }
    public string? ExpectedValue { get; set; }
    public ResolvedFailureContext Context { get; set; } = new();
}
