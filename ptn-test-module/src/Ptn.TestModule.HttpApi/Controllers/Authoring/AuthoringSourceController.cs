using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Authoring;
using SystemStandards.Results;

namespace Ptn.TestModule.Controllers.Authoring;

// islevi: Is kurali ve profil paketi malzemesinin yuklenmesini ve listelenmesini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve permission tasiyip her action'i tek AppService cagrisina yonlendirir.
[Route(AuthoringSourceRoutes.Root)]
[ApiExplorerSettings(GroupName = AuthoringSourceRoutes.SwaggerGroupName)]
public class AuthoringSourceController : TestModuleController
{
    private IAuthoringSourceAppService AppService =>
        LazyGetRequiredService<IAuthoringSourceAppService>();

    // Is kurali belgesini tek kaynaga yazar ve yazilan icerigin muhrunu dondurur.
    [HttpPost(AuthoringSourceRoutes.BusinessRules)]
    [Authorize(TestModulePermissions.Bridge.ManageSources)]
    public virtual async Task<Result<AuthoringSourceDto>> UploadBusinessRules(
        [FromBody] UploadBusinessRulesDto input)
    {
        var result = await AppService.UploadBusinessRulesAsync(input);
        return result;
    }

    // Profil paketini anahtarindan turetilen dosyaya yazar ve saklanan muhru dondurur.
    [HttpPost(AuthoringSourceRoutes.ProfilePacks)]
    [Authorize(TestModulePermissions.Bridge.ManageSources)]
    public virtual async Task<Result<AuthoringSourceDto>> UploadProfilePack(
        [FromBody] UploadProfilePackDto input)
    {
        var result = await AppService.UploadProfilePackAsync(input);
        return result;
    }

    // Yuklu profil paketlerini kapsam sayilariyla birlikte listeler.
    [HttpGet(AuthoringSourceRoutes.ProfilePacks)]
    [Authorize(TestModulePermissions.Bridge.ManageSources)]
    public virtual async Task<Result<List<ProfilePackSummaryDto>>> GetProfilePacks()
    {
        var result = await AppService.GetProfilePacksAsync();
        return result;
    }
}
