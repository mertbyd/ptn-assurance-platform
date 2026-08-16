namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Response conformance kural seviyelerini secen kapali profil kodlarini tanimlar.
// sistemdeki gorevi: HTTP girdisi ile domain policy resolver'ini kararli profil sozlesmesinde bulusturur.
public static class ConformanceProfileCodes
{
    public const string Strict = "Strict";
    public const string Runtime = "Runtime";
    public const string Lenient = "Lenient";

    public static IReadOnlyCollection<string> All { get; } = [Strict, Runtime, Lenient];
}
