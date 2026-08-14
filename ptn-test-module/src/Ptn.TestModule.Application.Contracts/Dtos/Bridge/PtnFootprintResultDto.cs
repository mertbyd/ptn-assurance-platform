namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Yazma kumesi gucu, nesne ozeti ve advisory bayragini public kontratta tasir.
// sistemdeki gorevi: Exact dahil footprint'in onaysiz assertion oracle'i olmadigini istemciye bildirir.
public sealed class PtnFootprintResultDto
{
    public string StrengthCode { get; set; } = string.Empty;
    public List<string> Tables { get; set; } = [];
    public List<string> Columns { get; set; } = [];
    public List<PtnRowDeltaDto> RowDeltas { get; set; } = [];
    public bool IsAdvisoryOnly { get; set; } = true;
    public List<string> Reasons { get; set; } = [];
}
