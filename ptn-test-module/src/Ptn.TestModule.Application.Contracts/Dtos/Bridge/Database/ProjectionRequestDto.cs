using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Anahtarla sinirli database projeksiyon istegini tasir.
// sistemdeki gorevi: Serbest SQL'i public Bridge yuzeyinin disinda tutar.
public sealed class ProjectionRequestDto
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
    /// <summary>
    /// Satiri adresleyen kolon-deger eslesmelerini belirtir.
    /// </summary>
    public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Isleme katilan kolon adlarini kararli sirada listeler.
    /// </summary>
    public List<string> ProjectColumns { get; set; } = [];
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int MaxRows { get; set; }
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}
