namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Tek tablonun capture oncesi ve sonrasi satir sayisi deltasini tasir.
// sistemdeki gorevi: Provider-notr checker ozetini public footprint sozlesmesinde korur.
public sealed class RowDeltaDto
{
    /// <summary>
    /// Hedef tablonun adini veya kararli adresini belirtir.
    /// </summary>
    public string Table { get; set; } = string.Empty;
    /// <summary>
    /// Capture oncesinde gozlenen satir sayisini belirtir.
    /// </summary>
    public long BeforeRowCount { get; set; }
    /// <summary>
    /// Capture sonrasinda gozlenen satir sayisini belirtir.
    /// </summary>
    public long AfterRowCount { get; set; }
    /// <summary>
    /// Onceki ve sonraki satir sayilari arasindaki farki belirtir.
    /// </summary>
    public long Delta { get; set; }
}
