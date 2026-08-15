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

// islevi: Veritabaninda hesaplanmis senaryo saglik ozetlerini HTTP'ye acar.
// sistemdeki gorevi: Route, binding ve operation permission'ini tasiyip her istegi tek AppService cagrisina yonlendirir.
/// <summary>Senaryo saglik HTTP okumalarini sunar.</summary>
[Route(ScenarioHealthRoutes.Root)]
[ApiExplorerSettings(GroupName = ScenarioHealthRoutes.SwaggerGroupName)]
public class ScenarioHealthController : TestModuleController
{
    /// <summary>Senaryo saglik AppService'ini lazy cozer.</summary>
    private IScenarioHealthAppService AppService => LazyGetRequiredService<IScenarioHealthAppService>();

    /// <summary>Senaryo saglik satirlarini filtreli ve kararli sayfalama ile getirir.</summary>
    /// <param name="input">Filtre, siralama ve sayfalama girdisi.</param>
    /// <returns>Ev standardi icinde pass, fail, flaky ve p95 ozetlerini tasiyan saglik sayfasi.</returns>
    [HttpGet]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<PagedResultDto<ScenarioHealthDto>>> GetList(
        [FromQuery] ScenarioHealthListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }

    /// <summary>Tek senaryo anahtarinin saglik ozetini getirir.</summary>
    /// <param name="scenarioKey">Sagligi okunacak kararli senaryo anahtari.</param>
    /// <returns>Ev standardi icinde tek senaryonun saglik ozeti.</returns>
    [HttpGet(ScenarioHealthRoutes.ByScenarioKey)]
    [Authorize(TestModulePermissions.Runs.View)]
    public virtual async Task<Result<ScenarioHealthDto>> GetByScenarioKey(string scenarioKey)
    {
        var result = await AppService.GetByScenarioKeyAsync(scenarioKey);
        return result;
    }
}
