using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Satir, sayim ve yokluk assertion girdisini tasir.
// sistemdeki gorevi: Database checker DTO'sunu public Test Module sozlesmesine sizdirmadan tasima siniri kurar.
public sealed class DatabaseAssertionRequestDto
{
    /// <summary>
    /// Checker isleminin calisacagi baglantinin kimligini belirtir.
    /// </summary>
    public Guid ConnectionId { get; set; }
    /// <summary>
    /// Hedef semanin kararli adini belirtir.
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;
    /// <summary>
    /// Hedef tablonun adini veya kararli adresini belirtir.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Satiri adresleyen kolon-deger eslesmelerini belirtir.
    /// </summary>
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Dogrulama veya assertion girdilerini kararli sirada listeler.
    /// </summary>
    public List<ColumnExpectationDto> Expectations { get; set; } = [];
    /// <summary>
    /// Satir sayisi beklentisini tasir.
    /// </summary>
    public DatabaseCardinalityExpectationDto Cardinality { get; set; } = new();
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int TimeoutMs { get; set; }
    /// <summary>
    /// Sozlesmenin poll interval ms bilgisini belirtir.
    /// </summary>
    public int PollIntervalMs { get; set; }
    /// <summary>
    /// Sozlesmenin include row on failure bilgisini belirtir.
    /// </summary>
    public bool IncludeRowOnFailure { get; set; } = true;
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}
