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

// islevi: Kosum durumu lookup okumalarini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve okuma permission'ini tasir; satirlar seed ile geldigi icin yazma ucu tanimlamaz.
/// <summary>Kosum durumu lookup HTTP okumalarini sunar.</summary>
[Route(TestLookupRoutes.RunStatuses)]
[ApiExplorerSettings(GroupName = TestLookupRoutes.SwaggerGroupName)]
public class TestRunStatusController : TestModuleController
{
    /// <summary>Kosum durumu lookup AppService'ini lazy cozer.</summary>
    private ITestRunStatusAppService AppService => LazyGetRequiredService<ITestRunStatusAppService>();

    /// <summary>Kimligi verilen kosum durumu satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Ev standardi icinde kosum durumu gorunumu.</returns>
    [HttpGet(TestLookupRoutes.ById)]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<TestRunStatusDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Kosum durumu satirlarini kararli sayfalama ile getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Ev standardi icinde kosum durumu sayfasi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<PagedResultDto<TestRunStatusDto>>> GetList(
        [FromQuery] LookupListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }
}
