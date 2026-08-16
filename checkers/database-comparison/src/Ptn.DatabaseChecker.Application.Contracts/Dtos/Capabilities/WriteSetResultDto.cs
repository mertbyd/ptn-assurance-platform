namespace Ptn.DatabaseChecker.Dtos.Capabilities;

// islevi: Advisory yazma kumesi tablo, kolon ve satir delta ozetini public cevapta tasir.
// sistemdeki gorevi: Ham WAL ve satir degeri sizdirmadan dort capability seviyesini ayni wire sekline baglar.
public sealed class WriteSetResultDto
{
    public string StrengthCode { get; set; } = string.Empty;
    public List<string> Tables { get; set; } = [];
    public List<string> Columns { get; set; } = [];
    public List<WriteSetTableDeltaDto> RowDeltas { get; set; } = [];
    public bool IsAdvisoryOnly { get; set; } = true;
    public List<string> Reasons { get; set; } = [];
}
