namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Tek tablo sema bilgisinin tipli adresini tasir.
// sistemdeki gorevi: Sema sorgusunu serbest sorgu yerine baglanti ve tablo kimligiyle sinirlar.
public sealed class TableQueryDto
{
    /// <summary>
    /// Checker isleminin calisacagi baglantinin kimligini belirtir.
    /// </summary>
    public Guid ConnectionId { get; set; }
    /// <summary>
    /// Hedef semanin kararli adini belirtir.
    /// </summary>
    public string DbSchemaName { get; set; } = string.Empty;
    /// <summary>
    /// Hedef tablonun adini veya kararli adresini belirtir.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
}
