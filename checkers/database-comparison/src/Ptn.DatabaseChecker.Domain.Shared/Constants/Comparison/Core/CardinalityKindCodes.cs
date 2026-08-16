namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Anahtarla secilen satir kumesinin desteklenen cardinality beklentilerini tanimlar.
// sistemdeki gorevi: Row, count, absent ve batch uclari ayni kapali cardinality sozlesmesini paylasir.
public static class CardinalityKindCodes
{
    public const string Exactly = "Exactly";
    public const string AtLeast = "AtLeast";
    public const string None = "None";

    // islevi: Bir cardinality kodunun assertion sozlesmesinde tanimli olup olmadigini bildirir.
    public static bool IsDefined(string? code)
        => code is Exactly or AtLeast or None;
}
