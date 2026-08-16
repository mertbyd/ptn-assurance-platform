using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Correlation;

namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Snapshot operasyonu ve gozlenen HTTP yanitini oracle endpointine tasir.
// sistemdeki gorevi: Test Module'un tek response assertion HTTP girdisidir.
public class ResponseConformanceDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public JsonElement? Body { get; set; }
    public string ProfileCode { get; set; } = ConformanceProfileCodes.Runtime;
    public CorrelationRefDto? Correlation { get; set; }
}
