namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Hedef operasyon response'u ve assertion JSON Pointer listesini G2 manager'ina tasir.
// sistemdeki gorevi: HTTP DTO'sunu Domain'den ayirarak turetilebilirlik hesabini saf sozlesme girdisiyle kurar.
public class AssertionDerivabilityRequest
{
    public string? OperationId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? StatusCode { get; set; }
    public string? MediaType { get; set; }
    public List<string> AssertionPaths { get; set; } = [];
}
