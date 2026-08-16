namespace Ptn.ApiContractChecker.Dtos.Conformance;

// islevi: Tek onceki operasyon ve aciklanabilir alan baglarini HTTP cikisina tasir.
public class OperationBindingSuggestionDto
{
    public string? SourceOperationId { get; set; }
    public string SourceMethod { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public List<OperationFieldBindingDto> Bindings { get; set; } = new();
    public int Score { get; set; }
}
