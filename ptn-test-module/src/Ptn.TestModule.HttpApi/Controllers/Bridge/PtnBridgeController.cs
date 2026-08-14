using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Bridge;
using SystemStandards.Results;

namespace Ptn.TestModule.Controllers.Bridge;

// islevi: Bridge agent use-case'lerini bes yetkili HTTP endpoint'iyle acar.
// sistemdeki gorevi: Route, binding ve operation permission'i tasiyip her istegi tek AppService cagrisina yonlendirir.
/// <summary>Deterministik Bridge agent islemleri.</summary>
[Route(PtnBridgeRoutes.Root)]
[ApiExplorerSettings(GroupName = PtnBridgeRoutes.SwaggerGroupName)]
public class PtnBridgeController : TestModuleController
{
    // Bridge use-case AppService'ini lazy cozer; controller business karari tasimaz.
    private IPtnBridgeAppService AppService => LazyGetRequiredService<IPtnBridgeAppService>();

    /// <summary>Operasyon zeminini tek birlesik cevapta getirir.</summary>
    /// <param name="input">Profil, checker referanslari ve istenen response formati.</param>
    /// <returns>Grounding karari ve kanit ozetini tasiyan ev standardi sonucu.</returns>
    [HttpPost(PtnBridgeRoutes.Ground)]
    [Authorize(TestModulePermissions.Bridge.Ground)]
    public virtual async Task<Result<PtnGroundResultDto>> Ground([FromBody] PtnGroundRequestDto input)
    {
        var result = await AppService.GroundAsync(input);
        return result;
    }

    /// <summary>Kanit zincirinin mekanik aciklama sonucunu getirir.</summary>
    /// <param name="input">Aciklanacak operasyon ve gozlenen sonuc bilgisi.</param>
    /// <returns>Kanit zincirini tasiyan ev standardi sonucu.</returns>
    [HttpPost(PtnBridgeRoutes.Explain)]
    [Authorize(TestModulePermissions.Bridge.Explain)]
    public virtual async Task<Result<PtnExplainResultDto>> Explain([FromBody] PtnExplainRequestDto input)
    {
        var result = await AppService.ExplainAsync(input);
        return result;
    }

    /// <summary>Assertion turetilebilirligini ve yayin kapisini denetler.</summary>
    /// <param name="input">Yayin kapisinda denetlenecek assertion referanslari.</param>
    /// <returns>Kapali validation kararini tasiyan ev standardi sonucu.</returns>
    [HttpPost(PtnBridgeRoutes.Validate)]
    [Authorize(TestModulePermissions.Bridge.Validate)]
    public virtual async Task<Result<PtnValidateResultDto>> Validate([FromBody] PtnValidateRequestDto input)
    {
        var result = await AppService.ValidateAsync(input);
        return result;
    }

    /// <summary>Profil kaynakli kavram kapsamini getirir.</summary>
    /// <param name="input">Profil, baglanti ve kapali kavram kodlari.</param>
    /// <returns>Profil kapsamini tasiyan ev standardi sonucu.</returns>
    [HttpPost(PtnBridgeRoutes.Knowledge)]
    [Authorize(TestModulePermissions.Bridge.Knowledge)]
    public virtual async Task<Result<PtnKnowledgeResultDto>> GetKnowledge([FromBody] PtnKnowledgeRequestDto input)
    {
        var result = await AppService.GetKnowledgeAsync(input);
        return result;
    }

    /// <summary>Aktif ve talep uzerine kesfedilen tool katalagunu getirir.</summary>
    /// <returns>Aktif Bridge tool listesini tasiyan ev standardi sonucu.</returns>
    [HttpGet(PtnBridgeRoutes.ToolCatalog)]
    [Authorize(TestModulePermissions.Bridge.Knowledge)]
    public virtual async Task<Result<PtnToolCatalogDto>> GetToolCatalog()
    {
        var result = await AppService.GetToolCatalogAsync();
        return result;
    }
}
