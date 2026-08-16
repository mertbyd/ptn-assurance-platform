namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Tek assertion JSON Pointer'i icin kapali G2 sonucunu tasir.
// sistemdeki gorevi: Deger veya implementation gozlemi tasimadan sozlesme oracle kararini acar.
public class AssertionDerivabilityItem
{
    public string JsonPointer { get; set; } = string.Empty;
    public string OutcomeCode { get; set; } = string.Empty;
}
