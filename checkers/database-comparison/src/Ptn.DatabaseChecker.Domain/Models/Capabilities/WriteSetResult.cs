namespace Ptn.DatabaseChecker.Models.Capabilities;

// islevi: Exact veya inferred gozlemden tureyen advisory yazma kumesi ozetini tasir.
// sistemdeki gorevi: Ham WAL, secret, satir degeri veya provider exception'i sizdirmadan istemciye tek sonuc sekli verir.
public sealed class WriteSetResult
{
    public string StrengthCode { get; set; } = string.Empty;
    public List<string> Tables { get; set; } = [];
    public List<string> Columns { get; set; } = [];
    public List<WriteSetTableDelta> RowDeltas { get; set; } = [];
    public bool IsAdvisoryOnly { get; set; } = true;
    public List<string> Reasons { get; set; } = [];
}
