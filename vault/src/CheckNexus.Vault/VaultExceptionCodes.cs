namespace CheckNexus.Vault;

// islevi: Vault provider sinirindaki guvenli ve kararlı hata kodlarini tanimlar.
// sistemdeki gorevi: HTTP response body, token veya secret degerini ust katmanlara sizdirmadan timeout/provider/payload hatalarini ayirir.
public static class VaultExceptionCodes
{
    private const string Prefix = "CheckNexus.Vault";

    public const string RequestFailed = $"{Prefix}:RequestFailed";
    public const string Unavailable = $"{Prefix}:Unavailable";
    public const string Timeout = $"{Prefix}:Timeout";
    public const string InvalidPayload = $"{Prefix}:InvalidPayload";
    public const string MissingToken = $"{Prefix}:MissingToken";
}
