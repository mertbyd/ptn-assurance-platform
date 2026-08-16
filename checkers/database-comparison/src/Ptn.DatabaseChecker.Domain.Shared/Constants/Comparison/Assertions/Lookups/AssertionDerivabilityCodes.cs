namespace Ptn.DatabaseChecker.Constants.Comparison.Assertions;

// islevi: Veritabani assertion turetilebilirlik kapisinin kapali sonuc kodlarini tanimlar.
// sistemdeki gorevi: Katalog, anahtar ve matcher-tip kararlarini mesaj metninden bagimsiz public sozlesmeye tasir.
public static class AssertionDerivabilityCodes
{
    public const string Derivable = "Derivable";
    public const string TableNotFound = "TableNotFound";
    public const string ColumnNotFound = "ColumnNotFound";
    public const string KeyNotUnique = "KeyNotUnique";
    public const string MatcherTypeMismatch = "MatcherTypeMismatch";

    public static IReadOnlyCollection<string> All { get; } =
        [Derivable, TableNotFound, ColumnNotFound, KeyNotUnique, MatcherTypeMismatch];
}
