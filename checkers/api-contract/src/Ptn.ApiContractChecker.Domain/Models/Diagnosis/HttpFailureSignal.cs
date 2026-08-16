using Ptn.ApiContractChecker.Models.Correlation;

namespace Ptn.ApiContractChecker.Models.Diagnosis;

// islevi: Basarisiz API adiminin response ve gonderilen request metadatasini govde tasimadan toplar.
// sistemdeki gorevi: Test Module sinyalini HTTP nesnesi, ham govde veya model karar yoluna sokmadan teshis cekirdegine verir.
public sealed class HttpFailureSignal
{
    public string? OperationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int? StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string? SentContentType { get; set; }
    public string? Accept { get; set; }
    public string? ConformanceOutcomeCode { get; set; }
    public string? TransportErrorCode { get; set; }
    public string? ProblemType { get; set; }
    public string? ProblemTitle { get; set; }
    public string? ProblemDetail { get; set; }
    public string? ProblemInstance { get; set; }
    public string? RemoteServiceErrorCode { get; set; }
    public string? ResponseVersion { get; set; }
    public string? ResourceUrl { get; set; }
    public long? ObservedAtMs { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> RequestHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ProblemErrorSignal> ProblemErrors { get; set; } = new();
    public CorrelationRef? Correlation { get; set; }
}
