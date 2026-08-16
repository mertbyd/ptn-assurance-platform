namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Hedefli tablo lint sonucunun kapali uyari kodlarini tanimlar.
// sistemdeki gorevi: Senaryo yayin kapisinin mesaj metnine veya provider ayrintisina baglanmadan tablo risklerini yorumlamasini saglar.
public static class SchemaLintWarningCodes
{
    public const string MissingPrimaryKey = "MissingPrimaryKey";
    public const string MissingUniqueKey = "MissingUniqueKey";
    public const string GeneratedColumn = "GeneratedColumn";

    public static IReadOnlyCollection<string> All { get; } =
        [MissingPrimaryKey, MissingUniqueKey, GeneratedColumn];
}
