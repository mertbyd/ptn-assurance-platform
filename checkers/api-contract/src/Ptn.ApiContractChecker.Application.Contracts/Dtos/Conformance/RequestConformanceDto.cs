using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Correlation;

namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Snapshot operasyonu ve gonderilecek HTTP istegini oracle endpointine tasir.
// sistemdeki gorevi: Test yazim aninin tek request assertion HTTP girdisidir.
public class RequestConformanceDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, string> Query { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string? ContentType { get; set; }
    public JsonElement? Body { get; set; }
    public string ProfileCode { get; set; } = ConformanceProfileCodes.Runtime;
    public CorrelationRefDto? Correlation { get; set; }
}
