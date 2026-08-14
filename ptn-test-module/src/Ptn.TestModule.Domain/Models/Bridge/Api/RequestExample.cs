using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Ptn.TestModule.Models.Bridge;

// islevi: API checker'in minimal request ornegini tipli JSON parcalariyla tasir.
// sistemdeki gorevi: Checker DTO'sundan bagimsiz modeli Mapperly ile kayipsiz tasir.
public sealed class RequestExample
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
