namespace Ptn.ApiContractChecker.Constants.Differences.Lookups;

// islevi: Bulgularin uyumluluk etkisini belirleyen kararli siddet kodlarini tanimlar.
// sistemdeki gorevi: Engine bulgulari, run sayaclari ve seed arasindaki tek kod sozlesmesidir.
public static class DifferenceSeverityCodes
{
    public const string Breaking = "breaking";
    public const string NonBreaking = "non-breaking";
    public const string DocsOnly = "docs-only";

    public static IReadOnlyCollection<string> All { get; } =
        [Breaking, NonBreaking, DocsOnly];
}
