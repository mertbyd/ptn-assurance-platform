using System.Text.Json.Nodes;

namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Minimal request iskeletini placeholder ve tamamlik isaretleriyle HTTP cikisina tasir.
public class RequestExampleDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public bool ValuesArePlaceholders { get; set; }
    public bool IsComplete { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, JsonNode?> PathParameters { get; set; } = new();
    public Dictionary<string, JsonNode?> Query { get; set; } = new();
    public Dictionary<string, JsonNode?> Headers { get; set; } = new();
    public JsonNode? Body { get; set; }
}
