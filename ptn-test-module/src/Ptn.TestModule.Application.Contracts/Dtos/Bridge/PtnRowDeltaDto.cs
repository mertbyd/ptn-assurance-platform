namespace Ptn.TestModule.Dtos.Bridge;

// islevi: Tek tablonun insert, update ve delete satir degisim sayilarini tasir.
// sistemdeki gorevi: Provider WAL kaydini public sozlesmeye sizdirmadan footprint ozetine ekler.
public sealed class PtnRowDeltaDto
{
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public long InsertedCount { get; set; }
    public long UpdatedCount { get; set; }
    public long DeletedCount { get; set; }
}
