using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Runs;
using SystemStandards.Results;

namespace Ptn.TestModule.Controllers.Runs;

// islevi: Ortam listesi ve sandbox reset use-case'lerini HTTP'ye acar.
// sistemdeki gorevi: Binding ve ayri permission metadata'sini AppService'e delege eder.
[Route(TestEnvironmentRoutes.Root)]
[ApiExplorerSettings(GroupName = TestEnvironmentRoutes.SwaggerGroupName)]
public class TestEnvironmentController : TestModuleController
{
    private ITestEnvironmentAppService AppService => LazyGetRequiredService<ITestEnvironmentAppService>();

    [HttpGet]
    [Authorize(TestModulePermissions.Runs.View)]
    public async Task<Result<List<TestEnvironmentBindingDto>>> GetList()
    {
        var result = await AppService.GetListAsync();
        return result;
    }

    [HttpPost(TestEnvironmentRoutes.SandboxReset)]
    [Authorize(TestModulePermissions.Runs.SandboxReset)]
    public async Task<Result> ResetSandbox(string key)
    {
        await AppService.ResetSandboxAsync(key);
        return Result.NoContent();
    }
}
