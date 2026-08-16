namespace Ptn.ApiContractChecker.Constants.Conformance.Lookups;

// islevi: Ornegin kisit eksenindeki konumunu kapali kod kumesinde tanimlar.
// sistemdeki gorevi: Sinir ve ihlal degerlerinin neden kabul ya da ret bekledigini metinden bagimsiz aciklar.
public static class SamplePositionCodes
{
    public const string BelowMin = "BelowMin";
    public const string AtMin = "AtMin";
    public const string AboveMin = "AboveMin";
    public const string BelowMax = "BelowMax";
    public const string AtMax = "AtMax";
    public const string AboveMax = "AboveMax";
    public const string Violation = "Violation";

    public static IReadOnlyCollection<string> All { get; } =
    [
        BelowMin, AtMin, AboveMin, BelowMax, AtMax, AboveMax, Violation
    ];
}
