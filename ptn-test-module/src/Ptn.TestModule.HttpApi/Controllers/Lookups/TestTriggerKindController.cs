using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Constants.Lookups;
using Ptn.TestModule.Dtos.Lookups;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Lookups;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Controllers.Lookups;

// islevi: Tetikleme turu lookup okumalarini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve okuma permission'ini tasir; satirlar seed ile geldigi icin yazma ucu tanimlamaz.
/// <summary>Tetikleme turu lookup HTTP okumalarini sunar.</summary>
[Route(TestLookupRoutes.TriggerKinds)]
[ApiExplorerSettings(GroupName = TestLookupRoutes.SwaggerGroupName)]
public class TestTriggerKindController : TestModuleController
{
    /// <summary>Tetikleme turu lookup AppService'ini lazy cozer.</summary>
    private ITestTriggerKindAppService AppService => LazyGetRequiredService<ITestTriggerKindAppService>();

    /// <summary>Kimligi verilen tetikleme turu satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Ev standardi icinde tetikleme turu gorunumu.</returns>
    [HttpGet(TestLookupRoutes.ById)]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<TestTriggerKindDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Tetikleme turu satirlarini kararli sayfalama ile getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Ev standardi icinde tetikleme turu sayfasi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<PagedResultDto<TestTriggerKindDto>>> GetList(
        [FromQuery] LookupListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }
}
