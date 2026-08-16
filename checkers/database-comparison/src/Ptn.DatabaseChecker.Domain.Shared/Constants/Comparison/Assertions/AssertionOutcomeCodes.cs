namespace Ptn.DatabaseChecker.Constants.Comparison.Assertions;

// islevi: Assertion sonucunun Test Module ve KBP-705 tarafindan yorumlanacak kararli sonuc kodlarini tanimlar.
// sistemdeki gorevi: Basari, veri uyusmazligi, bekleme ve katalog dogrulama sonuclari exception metnine baglanmadan tasinir.
public static class AssertionOutcomeCodes
{
    public const string Passed = "Passed";
    public const string RowNotFound = "RowNotFound";
    public const string ValueMismatch = "ValueMismatch";
    public const string CardinalityMismatch = "CardinalityMismatch";
    public const string TimedOut = "TimedOut";
    public const string KeyNotUnique = "KeyNotUnique";
    public const string TableNotFound = "TableNotFound";
    public const string ColumnNotFound = "ColumnNotFound";
}
