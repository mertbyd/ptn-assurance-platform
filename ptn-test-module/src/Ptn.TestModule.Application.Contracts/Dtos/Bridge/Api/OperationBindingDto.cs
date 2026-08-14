namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Normalize operasyon esleme sonucunu ve adaylarini tasir.
// sistemdeki gorevi: API checker ayrintilarini public Bridge sozlesmesinden gizler.
public sealed class OperationBindingDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<OperationSuggestionDto> Suggestions { get; set; } = [];
}
