namespace Ptn.TestModule.Models.Bridge.Footprint;

// islevi: Tek tablodaki insert, update ve delete satir degisim sayilarini tasir.
// sistemdeki gorevi: Yazma kumesi gozlemini ham WAL kaydi veya serbest provider verisi sizdirmadan ozetler.
public sealed class PtnRowDelta
{
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public long InsertedCount { get; set; }
    public long UpdatedCount { get; set; }
    public long DeletedCount { get; set; }
}
