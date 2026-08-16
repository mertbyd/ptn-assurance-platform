namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Deger tasimayan ve butceli G2 assertion sonuc listesini HTTP cevabinda tasir.
public class AssertionDerivabilityResultDto
{
    public List<AssertionDerivabilityItemDto> Assertions { get; set; } = [];
    public bool IsTruncated { get; set; }
}
