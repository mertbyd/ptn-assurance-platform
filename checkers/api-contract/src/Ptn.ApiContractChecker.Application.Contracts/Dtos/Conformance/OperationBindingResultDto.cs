namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Hedef operasyon icin sirali ve butcelenmis ODG onerilerini HTTP cikisina tasir.
public class OperationBindingResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<OperationBindingSuggestionDto> Suggestions { get; set; } = new();
}
