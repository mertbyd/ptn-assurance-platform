namespace Ptn.TestModule.ExceptionCodes.Compilation;

// islevi: Arazzo derleme ve Redocly lint sinirinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Yayin kapisinin parser veya surec mesajina baglanmadan hatayi siniflandirmasini saglar.
public static class TestModuleCompilationErrorCodes
{
    private const string Prefix = "TestModule.Compilation";

    public const string InvalidDocument = $"{Prefix}:InvalidDocument";
    public const string UnsupportedVersion = $"{Prefix}:UnsupportedVersion";
    public const string XPathCriteriaUnsupported = $"{Prefix}:XPathCriteriaUnsupported";
    public const string DatabaseSourceDescriptionMissing = $"{Prefix}:DatabaseSourceDescriptionMissing";
    public const string UnsupportedDatabaseOperation = $"{Prefix}:UnsupportedDatabaseOperation";
    public const string InvalidDatabaseAssertion = $"{Prefix}:InvalidDatabaseAssertion";
    public const string ConceptColumnNotBound = $"{Prefix}:ConceptColumnNotBound";
    public const string LintProcessFailed = $"{Prefix}:LintProcessFailed";
    public const string LintTimedOut = $"{Prefix}:LintTimedOut";
}
