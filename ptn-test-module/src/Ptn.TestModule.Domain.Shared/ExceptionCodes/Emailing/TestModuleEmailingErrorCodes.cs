namespace Ptn.TestModule.ExceptionCodes.Emailing;

// islevi: Emailing secret adapterinin guvenli ve kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Vault tokenini veya response govdesini sizdirmadan provider sorunlarini ayirir.
public static class TestModuleEmailingErrorCodes
{
    private const string Prefix = "TestModule.Emailing";

    public const string VaultRequestFailed = $"{Prefix}:VaultRequestFailed";
    public const string VaultUnavailable = $"{Prefix}:VaultUnavailable";
    public const string VaultTimeout = $"{Prefix}:VaultTimeout";
    public const string VaultInvalidPayload = $"{Prefix}:VaultInvalidPayload";
    public const string VaultPathInvalid = $"{Prefix}:VaultPathInvalid";
    public const string VaultMissingToken = $"{Prefix}:VaultMissingToken";
}
