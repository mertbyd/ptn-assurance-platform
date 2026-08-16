namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Sema ornegi uretim turlerini kapali kod kumesinde tanimlar.
// sistemdeki gorevi: Public input ile iki ureticinin ayni kararli secim sozlesmesini kullanmasini saglar.
public static class SampleKindCodes
{
    public const string Boundary = "Boundary";
    public const string Negative = "Negative";
    public const string Both = "Both";

    public static IReadOnlyCollection<string> All { get; } = [Boundary, Negative, Both];
}
