using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: API checker'in minimal request ornegini JSON metin parcalariyla tasir.
// sistemdeki gorevi: JsonNode ve checker DTO'sunu domain sinirina sokmadan yazarlik olgusu verir.
public sealed class PtnRequestExample
{
    public string OutcomeCode { get; set; } = string.Empty;
    public bool ValuesArePlaceholders { get; set; }
    public bool IsComplete { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string?> PathParameters { get; set; } = [];
    public Dictionary<string, string?> Query { get; set; } = [];
    public Dictionary<string, string?> Headers { get; set; } = [];
    public string? BodyJson { get; set; }
}
