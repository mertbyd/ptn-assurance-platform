namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Deger tasimayan kural, JSON pointer ve schema keyword ihlal ucusunu tasir.
public class ConformanceViolationDto
{
    public string RuleCode { get; set; } = string.Empty;
    public string JsonPointer { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
}
