using System;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Dtos.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Lookups;

// islevi: Senaryo yayin durumu lookup'inin salt-okunur use-case'lerini tanimlar.
// sistemdeki gorevi: Satirlar seed ile geldigi icin sozlesme yalniz okuma tasir; yazma ucu bilerek acilmaz.
/// <summary>Senaryo yayin durumu lookup okumalarinin Application sozlesmesidir.</summary>
public interface ITestScenarioStateAppService : IApplicationService
{
    /// <summary>Kimligi verilen senaryo yayin durumu satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Senaryo yayin durumu lookup gorunumu.</returns>
    Task<TestScenarioStateDto> GetAsync(Guid id);

    /// <summary>Senaryo yayin durumu satirlarini kararli siralama ve sayfalamayla getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Senaryo yayin durumu lookup sayfasi.</returns>
    Task<PagedResultDto<TestScenarioStateDto>> GetListAsync(LookupListInput input);
}
