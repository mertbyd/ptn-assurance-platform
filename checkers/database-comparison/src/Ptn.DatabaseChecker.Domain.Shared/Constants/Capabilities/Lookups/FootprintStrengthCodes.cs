namespace Ptn.DatabaseChecker.Constants.Capabilities;

// islevi: Yazma kumesi ayak izinin kapali guc seviyelerini tanimlar.
// sistemdeki gorevi: Capability yoklama ve capture sonucunu serbest mesajlardan bagimsiz public kodlara baglar.
public static class FootprintStrengthCodes
{
    public const string Exact = "Exact";
    public const string RowAddressed = "RowAddressed";
    public const string Inferred = "Inferred";
    public const string Unavailable = "Unavailable";

    public static IReadOnlyCollection<string> All { get; } =
        [Exact, RowAddressed, Inferred, Unavailable];
}
