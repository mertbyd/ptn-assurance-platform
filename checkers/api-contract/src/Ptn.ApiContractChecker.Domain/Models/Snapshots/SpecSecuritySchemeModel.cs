namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir guvenlik semasi adini ve istenen scope listesini tasir.
// sistemdeki gorevi: Security requirement siralamasini sema ve scope kimlikleriyle deterministik hale getirir.
public class SpecSecuritySchemeModel
{
    // Components securitySchemes altindaki sema adi.
    public string Name { get; set; } = string.Empty;

    // HTTP auth scheme degeri; ornegin bearer.
    public string? Scheme { get; set; }

    // OAuth/OpenID semasinda operasyonun istedigi scope adlari.
    public List<string> Scopes { get; set; } = new();
}
