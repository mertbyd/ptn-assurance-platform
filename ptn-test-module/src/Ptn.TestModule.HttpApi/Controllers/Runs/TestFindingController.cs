using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Runs;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Controllers.Runs;

// islevi: Kosumlar arasi bulgu sorgusunu HTTP'ye acar.
// sistemdeki gorevi: Binding ve authorization tasiyip AppService'e dogrudan delege eder.
[Route(TestFindingRoutes.Root)]
[ApiExplorerSettings(GroupName = TestFindingRoutes.SwaggerGroupName)]
public class TestFindingController : TestModuleController
{
    private ITestFindingAppService AppService => LazyGetRequiredService<ITestFindingAppService>();

    [HttpGet]
    [Authorize(TestModulePermissions.Runs.View)]
    public async Task<Result<PagedResultDto<TestFindingHeaderDto>>> GetList([FromQuery] TestFindingListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }
}
