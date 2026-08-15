namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Kapali operation link sonucunu ve kanitli adaylarini public Bridge cevabinda tasir.
// sistemdeki gorevi: Checker DTO ailesinin tuketici ve MCP sinirina sizmasini engeller.
public sealed class OperationLinkResultDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public List<OperationLinkCandidateDto> Candidates { get; set; } = [];
}
