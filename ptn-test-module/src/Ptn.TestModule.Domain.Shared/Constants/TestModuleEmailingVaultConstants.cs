namespace Ptn.TestModule.Constants;

// islevi: Emailing Vault adapterinin HTTP, payload ve hata metadata sozlesmesindeki kararli adlari toplar.
// sistemdeki gorevi: Secret wire semasini composition kodundaki literal stringlerden ayirir.
public static class TestModuleEmailingVaultConstants
{
    public const string HttpClientName = "Ptn.TestModule.Emailing.Vault";
    public const string TokenHeaderName = "X-Vault-Token";
    public const string NamespaceHeaderName = "X-Vault-Namespace";
    public const string DataPropertyName = "data";
    public const string SmtpUsernamePropertyName = "smtpUsername";
    public const string SmtpPasswordPropertyName = "smtpPassword";
    public const string GoogleClientSecretPropertyName = "googleClientSecret";
    public const string GoogleRefreshTokenPropertyName = "googleRefreshToken";
    public const string StatusCodeDataKey = "StatusCode";
}
