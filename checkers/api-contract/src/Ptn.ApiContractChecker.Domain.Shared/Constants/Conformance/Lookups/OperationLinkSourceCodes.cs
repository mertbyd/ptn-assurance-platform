namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Operasyon zinciri adayinin mekanik kaynagini kapali kod kumesinde tanimlar.
// sistemdeki gorevi: Aday skorunun beyan, sema veya Location kanitindan hangisine dayandigini aciklar.
public static class OperationLinkSourceCodes
{
    public const string DeclaredLink = "DeclaredLink";
    public const string SchemaMatch = "SchemaMatch";
    public const string LocationHeader = "LocationHeader";

    public static IReadOnlyCollection<string> All { get; } =
        [DeclaredLink, SchemaMatch, LocationHeader];
}
