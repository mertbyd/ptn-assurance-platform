namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Snapshot operasyonu, response secimi ve assertion JSON Pointer listesini G2 HTTP ucuna tasir.
public class AssertionDerivabilityDto
{
    public Guid SnapshotId { get; set; }
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? StatusCode { get; set; }
    public string? MediaType { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
