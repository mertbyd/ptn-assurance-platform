using System.Text.Json.Nodes;

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Sozlesmeden uretilen request ornegini ve tamamlanma durumunu tasir.
// sistemdeki gorevi: Placeholder isaretli ornegi checker DTO'sundan bagimsiz sunar.
public sealed class RequestExampleDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public bool ValuesArePlaceholders { get; set; }
    public bool IsComplete { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, JsonNode?> PathParameters { get; set; } = [];
    public Dictionary<string, JsonNode?> Query { get; set; } = [];
    public Dictionary<string, JsonNode?> Headers { get; set; } = [];
    public JsonNode? Body { get; set; }
}
