using Ptn.TestModule.Dtos.Bridge.Api;

namespace Ptn.TestModule.Dtos.Bridge;

// islevi: ptn_validate yayin karari, kapsam ve turetilebilirlik ayrintisini tasir.
// sistemdeki gorevi: Spec veya referans boslugunda kapali Inconclusive sonucunu Allow gibi gostermeyi engeller.
public sealed class PtnValidateResultDto
{
    public string ResponseFormat { get; set; } = string.Empty;
    public PtnCoverageReportDto Coverage { get; set; } = new();
    public bool IsPublishable { get; set; }
    public string DecisionCode { get; set; } = string.Empty;
    public DerivabilityResultDto? Derivability { get; set; }
    public List<PtnClosedQuestionDto> Questions { get; set; } = [];
    public string? ResourceLink { get; set; }
}
