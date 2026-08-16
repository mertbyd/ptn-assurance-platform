using System.Text.Json;
using System.Text.Json.Nodes;
using Ptn.ApiContractChecker.Constants.Conformance;

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Operasyonun minimal request iskeletini ve placeholder niteliklerini tasir.
// sistemdeki gorevi: Test yazarina 2 KB altinda, is degeri uydurmayan bir baslangic payload'i verir.
public sealed class RequestExampleResult
{
    public string OutcomeCode { get; }
    public bool ValuesArePlaceholders { get; } = true;
    public bool IsComplete { get; private set; } = true;
    public string? ContentType { get; }
    public Dictionary<string, JsonNode?> PathParameters { get; private set; }
    public Dictionary<string, JsonNode?> Query { get; private set; }
    public Dictionary<string, JsonNode?> Headers { get; private set; }
    public JsonNode? Body { get; private set; }

    public RequestExampleResult(
        string outcomeCode,
        string? contentType,
        Dictionary<string, JsonNode?> pathParameters,
        Dictionary<string, JsonNode?> query,
        Dictionary<string, JsonNode?> headers,
        JsonNode? body)
    {
        OutcomeCode = outcomeCode;
        ContentType = contentType;
        PathParameters = pathParameters;
        Query = query;
        Headers = headers;
        Body = body;
    }

    // Zorunlu iskelet 2 KB'a sigmiyorsa degerleri cikartip eksikligi acikca isaretler.
    public void TrimToBudget()
    {
        if (MeasureUtf8Bytes() <= ConformanceAuthoringConstants.MaxRequestExampleBytes)
        {
            return;
        }

        IsComplete = false;
        PathParameters = new Dictionary<string, JsonNode?>();
        Query = new Dictionary<string, JsonNode?>();
        Headers = new Dictionary<string, JsonNode?>();
        Body = null;
    }

    public int MeasureUtf8Bytes()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this).Length;
    }
}
