namespace Ptn.ApiContractChecker.Constants.Differences.Lookups;

// islevi: Fark siddeti lookup satirlarinin varsayilan gorunen adlarini tanimlar.
// sistemdeki gorevi: Seed metinlerini Domain.Shared altinda tek kaynakta tutar.
public static class DifferenceSeverityNames
{
    public const string Breaking = "Breaking";
    public const string NonBreaking = "Non-breaking";
    public const string DocsOnly = "Documentation only";
}
