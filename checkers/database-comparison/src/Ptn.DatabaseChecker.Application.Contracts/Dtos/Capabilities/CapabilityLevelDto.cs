namespace Ptn.DatabaseChecker.Dtos.Capabilities;

// islevi: Hedefin yazma kumesi gozlem seviyesini ve dusurme gerekcelerini public API'de tasir.
// sistemdeki gorevi: Dort guc seviyesini ayni response sekliyle Test Module tuketicisine sunar.
public sealed class CapabilityLevelDto
{
    public string StrengthCode { get; set; } = string.Empty;
    public bool HasLogicalDecoding { get; set; }
    public bool HasExclusiveSandbox { get; set; }
    public List<string> Reasons { get; set; } = [];
}
