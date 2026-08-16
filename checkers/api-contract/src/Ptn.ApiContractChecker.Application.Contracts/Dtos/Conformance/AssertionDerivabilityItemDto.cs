namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Tek assertion yolu ve kapali G2 sonucunu HTTP cevabinda tasir.
public class AssertionDerivabilityItemDto
{
    public string JsonPointer { get; set; } = string.Empty;
    public string OutcomeCode { get; set; } = string.Empty;
}
