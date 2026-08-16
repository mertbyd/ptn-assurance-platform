using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Models.Correlation;

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Gonderilecek HTTP isteginin path/query/header/content/body gozlemini tasir.
// sistemdeki gorevi: Yazim aninda request uygunlugunu response oracle'inin ayni politika ve kurallariyla denetletir.
public sealed class RequestConformanceRequest : IConformanceObservation
{
    public string? OperationId { get; }
    public string Method { get; }
    public string Path { get; }
    public IReadOnlyDictionary<string, string> Query { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public string? ContentType { get; }
    public JsonElement? Body { get; }
    public string ProfileCode { get; }
    public CorrelationRef? Correlation { get; set; }

    public RequestConformanceRequest(
        string? operationId,
        string method,
        string path,
        IReadOnlyDictionary<string, string> query,
        IReadOnlyDictionary<string, string> headers,
        string? contentType,
        JsonElement? body,
        string profileCode = ConformanceProfileCodes.Runtime)
    {
        OperationId = operationId;
        Method = method;
        Path = path;
        Query = query;
        Headers = headers;
        ContentType = contentType;
        Body = body;
        ProfileCode = profileCode;
    }
}
