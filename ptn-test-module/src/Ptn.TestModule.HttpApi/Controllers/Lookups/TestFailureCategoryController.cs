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

// islevi: Bulgu kategorisi lookup okumalarini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve okuma permission'ini tasir; satirlar seed ile geldigi icin yazma ucu tanimlamaz.
/// <summary>Bulgu kategorisi lookup HTTP okumalarini sunar.</summary>
[Route(TestLookupRoutes.FailureCategories)]
[ApiExplorerSettings(GroupName = TestLookupRoutes.SwaggerGroupName)]
public class TestFailureCategoryController : TestModuleController
{
    /// <summary>Bulgu kategorisi lookup AppService'ini lazy cozer.</summary>
    private ITestFailureCategoryAppService AppService => LazyGetRequiredService<ITestFailureCategoryAppService>();

    /// <summary>Kimligi verilen bulgu kategorisi satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Ev standardi icinde bulgu kategorisi gorunumu.</returns>
    [HttpGet(TestLookupRoutes.ById)]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<TestFailureCategoryDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Bulgu kategorisi satirlarini kararli sayfalama ile getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Ev standardi icinde bulgu kategorisi sayfasi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Lookups.Default)]
    public virtual async Task<Result<PagedResultDto<TestFailureCategoryDto>>> GetList(
        [FromQuery] LookupListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }
}
