namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_explain kapsamini, hukmunu, kritik olgusunu ve aciklama agacini tasir.
// sistemdeki gorevi: Concise cevapta karari once verip agir agaci ResourceLink arkasina tasiyabilir.
public sealed class PtnExplainResultDto
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReportDto Coverage { get; set; } = new();
    public string VerdictCode { get; set; } = string.Empty;
    public string CriticalFactCode { get; set; } = string.Empty;
    public PtnExplanationNodeDto? Root { get; set; }
    public List<PtnClosedQuestionDto> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
