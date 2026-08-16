using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Models.Correlation;

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Operasyon kimligi ile gozlenen HTTP yanitini deger tasimayan oracle girdisinde toplar.
// sistemdeki gorevi: Application DTO'sunu transport ayrintilarindan arinmis tek domain degerine indirger.
public sealed class ResponseConformanceRequest : IConformanceObservation
{
    public string? OperationId { get; }
    public string Method { get; }
    public string Path { get; }
    public int StatusCode { get; }
    public string? ContentType { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public JsonElement? Body { get; }
    public string ProfileCode { get; }
    public CorrelationRef? Correlation { get; set; }

    public ResponseConformanceRequest(
        string? operationId,
        string method,
        string path,
        int statusCode,
        string? contentType,
        IReadOnlyDictionary<string, string> headers,
        JsonElement? body,
        string profileCode = ConformanceProfileCodes.Runtime)
    {
        OperationId = operationId;
        Method = method;
        Path = path;
        StatusCode = statusCode;
        ContentType = contentType;
        Headers = headers;
        Body = body;
        ProfileCode = profileCode;
    }
}
