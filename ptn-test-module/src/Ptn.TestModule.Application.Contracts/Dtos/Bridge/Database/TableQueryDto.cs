namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek tablo sema bilgisinin tipli adresini tasir.
// sistemdeki gorevi: Sema sorgusunu serbest sorgu yerine baglanti ve tablo kimligiyle sinirlar.
public sealed class TableQueryDto
{
    public Guid ConnectionId { get; set; }
    public string DbSchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
}
