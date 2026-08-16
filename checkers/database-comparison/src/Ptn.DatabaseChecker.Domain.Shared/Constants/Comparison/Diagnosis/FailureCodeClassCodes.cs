namespace Ptn.DatabaseChecker.Constants.Diagnosis;

// islevi: Cikarilan hata kodunun assertion, SQLSTATE sinifi veya SQL Server numarasi ailesini tanimlar.
// sistemdeki gorevi: Provider kodunu hipotez uygulanabilirlik olgularindan ayirarak raporda yapilandirilmis kimlik tasir.
public static class FailureCodeClassCodes
{
    public const string Assertion = "Assertion";
    public const string IntegrityConstraint = "IntegrityConstraint";
    public const string SqlState = "SqlState";
    public const string SqlServerNumber = "SqlServerNumber";
}
