namespace Ptn.TestModule.Models.Bridge.Footprint;

// islevi: Tek tablonun capture oncesi ve sonrasi satir sayisi deltasini tasir.
// sistemdeki gorevi: Checker'in provider-notr row delta ozetini ham WAL verisi sizdirmadan korur.
public sealed class RowDelta
{
    public string Table { get; set; } = string.Empty;
    public long BeforeRowCount { get; set; }
    public long AfterRowCount { get; set; }
    public long Delta { get; set; }
}
