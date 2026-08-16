namespace Ptn.DatabaseChecker.ExceptionCodes;

public static class ComparisonExceptionCodes
{
    private const string Prefix = "DatabaseChecker.Comparison";

    public const string UnsupportedEngine = $"{Prefix}:00001";

    // Istenen sema adi hedefin kullanici sema katalogunda bulunmadiginda firlatilir; eksik semayi sessizce atlayan bir muhur uretilmez.
    public const string SchemaNotFound = $"{Prefix}:SchemaNotFound";
}
