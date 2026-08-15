using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Runs;

// islevi: Senaryo saglik okuma use-case'lerini tanimlar.
// sistemdeki gorevi: Materialized view'i acmadan Application katmaninin salt-okunur saglik sozlesmesini sunar.
public interface IScenarioHealthAppService : IApplicationService
{
    /// <summary>Senaryo saglik satirlarini filtreli ve kararli sayfalama ile getirir.</summary>
    Task<PagedResultDto<ScenarioHealthDto>> GetListAsync(ScenarioHealthListInput input);

    /// <summary>Tek senaryo anahtarinin saglik ozetini getirir.</summary>
    Task<ScenarioHealthDto> GetByScenarioKeyAsync(string scenarioKey);
}
