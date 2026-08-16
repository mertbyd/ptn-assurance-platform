namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Tek operasyonun butceli request, response ve security ozetini HTTP cevabinda tasir.
public class OperationSummaryDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? OperationId { get; set; }
    public List<OperationParameterSummaryDto> RequiredParameters { get; set; } = [];
    public List<string> RequestMediaTypes { get; set; } = [];
    public List<string> SuccessStatusCodes { get; set; } = [];
    public List<SchemaFieldSummaryDto> ResponseFields { get; set; } = [];
    public List<string> SecurityRequirements { get; set; } = [];
    public bool IsTruncated { get; set; }
    public string? ResultRef { get; set; }
}
