using Ptn.TestModule.Dtos.Bridge;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Ajanin grounding, explanation, validation, knowledge ve tool katalog use-case'lerini tanimlar.
// sistemdeki gorevi: Dar Bridge yuzeyini HTTP ve MCP composition katmanlarindan ayirir.
public interface IPtnBridgeAppService : IApplicationService
{
    Task<PtnGroundResultDto> GroundAsync(PtnGroundRequestDto input);
    Task<PtnExplainResultDto> ExplainAsync(PtnExplainRequestDto input);
    Task<PtnValidateResultDto> ValidateAsync(PtnValidateRequestDto input);
    Task<PtnKnowledgeResultDto> GetKnowledgeAsync(PtnKnowledgeRequestDto input);
    Task<PtnToolCatalogDto> GetToolCatalogAsync();
}
