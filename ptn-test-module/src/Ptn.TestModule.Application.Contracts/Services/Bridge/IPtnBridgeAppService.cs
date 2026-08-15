using Ptn.TestModule.Dtos.Bridge;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Ajanin grounding, explanation, validation, knowledge ve tool katalog use-case'lerini tanimlar.
// sistemdeki gorevi: Dar Bridge yuzeyini HTTP ve MCP composition katmanlarindan ayirir.
public interface IPtnBridgeAppService : IApplicationService
{
    Task<GroundResultDto> GroundAsync(GroundRequestDto input);
    Task<ExplainResultDto> ExplainAsync(ExplainRequestDto input);
    Task<ValidateResultDto> ValidateAsync(ValidateRequestDto input);
    Task<KnowledgeResultDto> GetKnowledgeAsync(KnowledgeRequestDto input);
    Task<ToolCatalogDto> GetToolCatalogAsync();
    Task<AgentProfileDto> ResolveAgentProfileAsync(AgentProfileRequestDto input);
    Task<ToolBudgetDecisionDto> CheckToolBudgetAsync(ToolBudgetRequestDto input);
    Task<McpTaskStatusDto> MapTaskStatusAsync(McpTaskStatusRequestDto input);
    Task<OverlayPatchSuggestionDto> SuggestOverlayPatchAsync(OverlayPatchRequestDto input);
}
