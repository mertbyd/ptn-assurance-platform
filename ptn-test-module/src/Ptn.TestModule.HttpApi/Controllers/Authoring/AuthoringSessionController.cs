using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Authoring;
using SystemStandards.Results;

namespace Ptn.TestModule.Controllers.Authoring;

// islevi: Tenant authoring session baslatma, okuma, cevaplama ve tek-adim islemlerini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve permission tasiyip her action'i tek AppService cagrisina yonlendirir.
[Route(AuthoringSessionRoutes.Root)]
[ApiExplorerSettings(GroupName = AuthoringSessionRoutes.SwaggerGroupName)]
public class AuthoringSessionController : TestModuleController
{
    private IAuthoringSessionAppService AppService =>
        LazyGetRequiredService<IAuthoringSessionAppService>();

    // Gercek grounding sonucuyla yeni tenant authoring session'i baslatir.
    [HttpPost]
    [Authorize(TestModulePermissions.Scenarios.Create)]
    public virtual async Task<Result<AuthoringSessionDto>> Create(
        [FromBody] CreateAuthoringSessionDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }

    // Cache'teki guncel authoring session durumunu getirir.
    [HttpGet(AuthoringSessionRoutes.ById)]
    [Authorize(TestModulePermissions.Scenarios.Update)]
    public virtual async Task<Result<AuthoringSessionDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    // Tek kapali sorunun secilmis cevabini session'a yazar.
    [HttpPost(AuthoringSessionRoutes.Answer)]
    [Authorize(TestModulePermissions.Scenarios.Update)]
    public virtual async Task<Result<AuthoringSessionDto>> Answer(
        Guid id,
        [FromBody] AnswerAuthoringSessionDto input)
    {
        var result = await AppService.AnswerAsync(id, input);
        return result;
    }

    // Tek yapilandirilmis adimi mekanik Arazzo belgesine birlestirir.
    [HttpPost(AuthoringSessionRoutes.Step)]
    [Authorize(TestModulePermissions.Scenarios.Update)]
    public virtual async Task<Result<AuthoringSessionDto>> AddStep(
        Guid id,
        [FromBody] AddAuthoringStepDto input)
    {
        var result = await AppService.AddStepAsync(id, input);
        return result;
    }
}
