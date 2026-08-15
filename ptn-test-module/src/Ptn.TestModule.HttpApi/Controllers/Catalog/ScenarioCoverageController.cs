using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Catalog;
using SystemStandards.Results;

namespace Ptn.TestModule.Controllers.Catalog;

// islevi: Yayinlanmis senaryolarin kapsam raporunu HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve operation permission'ini tasiyip istegi tek AppService cagrisina yonlendirir.
/// <summary>Senaryo kapsam raporu HTTP okumasini sunar.</summary>
[Route(ScenarioCoverageConsts.Root)]
[ApiExplorerSettings(GroupName = ScenarioCoverageConsts.SwaggerGroupName)]
public class ScenarioCoverageController : TestModuleController
{
    /// <summary>Kapsam raporu AppService'ini lazy cozer.</summary>
    private IScenarioCoverageAppService AppService => LazyGetRequiredService<IScenarioCoverageAppService>();

    /// <summary>Yayinlanmis senaryolarin dokundugu operasyon ve kural kumelerini getirir.</summary>
    /// <returns>Ev standardi icinde kapsam payini tasiyan rapor; payda bilinmiyor olarak isaretlidir.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Scenarios.Default)]
    public virtual async Task<Result<ScenarioCoverageReportDto>> GetCoverage()
    {
        var result = await AppService.GetCoverageAsync();
        return result;
    }
}
