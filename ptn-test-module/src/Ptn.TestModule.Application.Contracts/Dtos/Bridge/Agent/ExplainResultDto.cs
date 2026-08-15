namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_explain kapsamini, hukmunu, kritik olgusunu ve aciklama agacini tasir.
// sistemdeki gorevi: Concise cevapta karari once verip agir agaci ResourceLink arkasina tasiyabilir.
public sealed class ExplainResultDto
{
    /// <summary>
    /// Cevabin concise veya ayrintili sunum bicimini belirtir.
    /// </summary>
    public string ResponseFormat { get; set; } = string.Empty;
    /// <summary>
    /// Profilin kavram kapsama ozetini tasir.
    /// </summary>
    public CoverageReportDto Coverage { get; set; } = new();
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string VerdictCode { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string CriticalFactCode { get; set; } = string.Empty;
    /// <summary>
    /// Aciklama agacinin kok dugumunu tasir.
    /// </summary>
    public ExplanationNodeDto? Root { get; set; }
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<ClosedQuestionDto> Questions { get; set; } = [];
    /// <summary>
    /// Ayrintili kaynaga erisim adresini belirtir.
    /// </summary>
    public string? ResourceLink { get; set; }
}
