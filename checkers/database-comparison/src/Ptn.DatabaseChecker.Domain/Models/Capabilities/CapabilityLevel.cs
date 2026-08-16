namespace Ptn.DatabaseChecker.Models.Capabilities;

// islevi: Hedef veritabaninin yazma kumesi gozlem gucunu ve olculen olgularini tasir.
// sistemdeki gorevi: Exact, inferred ve unavailable yollarini ayni kapali sonuc sozlesmesinde birlestirir.
public sealed class CapabilityLevel
{
    public string StrengthCode { get; set; } = string.Empty;
    public bool HasLogicalDecoding { get; set; }
    public bool HasExclusiveSandbox { get; set; }
    public List<string> Reasons { get; set; } = [];
}
