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

// islevi: Test hukmu lookup okumalarini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve okuma permission'ini tasir; satirlar seed ile geldigi icin yazma ucu tanimlamaz.
/// <summary>Test hukmu lookup HTTP okumalarini sunar.</summary>
[Route(TestLookupRoutes.OutcomeStatuses)]
[ApiExplorerSettings(GroupName = TestLookupRoutes.SwaggerGroupName)]
public class TestOutcomeStatusController : TestModuleController
{
    /// <summary>Test hukmu lookup AppService'ini lazy cozer.</summary>
    private ITestOutcomeStatusAppService AppService => LazyGetRequiredService<ITestOutcomeStatusAppService>();

    /// <summary>Kimligi verilen test hukmu satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Ev standardi icinde build politikasini da tasiyan test hukmu gorunumu.</returns>
    [HttpGet(TestLookupRoutes.ById)]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<TestOutcomeStatusDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Test hukmu satirlarini kararli sayfalama ile getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Ev standardi icinde test hukmu sayfasi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<PagedResultDto<TestOutcomeStatusDto>>> GetList(
        [FromQuery] LookupListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }
}
