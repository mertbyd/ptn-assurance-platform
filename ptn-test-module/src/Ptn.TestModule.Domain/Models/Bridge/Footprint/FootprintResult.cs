using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Footprint;

// islevi: Ortam yetenegine gore elde edilen advisory yazma kumesi gozlemini tasir.
// sistemdeki gorevi: Exact dahil hicbir gozlemin onaysiz assertion oracle'ina donusmemesini sozlesmede sabitler.
public sealed class FootprintResult
{
    public string StrengthCode { get; set; } = string.Empty;
    public List<string> Tables { get; set; } = [];
    public List<string> Columns { get; set; } = [];
    public List<RowDelta> RowDeltas { get; set; } = [];
    public bool IsAdvisoryOnly { get; set; } = true;
    public List<string> Reasons { get; set; } = [];
}
