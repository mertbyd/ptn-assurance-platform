using System.Text.Json.Nodes;

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Sozlesmeden uretilen request ornegini ve tamamlanma durumunu tasir.
// sistemdeki gorevi: Placeholder isaretli ornegi checker DTO'sundan bagimsiz sunar.
public sealed class RequestExampleDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string OutcomeCode { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool ValuesArePlaceholders { get; set; }
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool IsComplete { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string? ContentType { get; set; }
    /// <summary>
    /// Istek veya provider tarafindaki ad-deger alanlarini belirtir.
    /// </summary>
    public Dictionary<string, JsonNode?> PathParameters { get; set; } = [];
    /// <summary>
    /// Istek veya provider tarafindaki ad-deger alanlarini belirtir.
    /// </summary>
    public Dictionary<string, JsonNode?> Query { get; set; } = [];
    /// <summary>
    /// Istek veya provider tarafindaki ad-deger alanlarini belirtir.
    /// </summary>
    public Dictionary<string, JsonNode?> Headers { get; set; } = [];
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public JsonNode? Body { get; set; }
}
