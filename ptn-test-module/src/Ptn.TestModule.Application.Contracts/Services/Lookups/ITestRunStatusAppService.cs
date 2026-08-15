using System;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Dtos.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Lookups;

// islevi: Kosum durumu lookup'inin salt-okunur use-case'lerini tanimlar.
// sistemdeki gorevi: Satirlar seed ile geldigi icin sozlesme yalniz okuma tasir; yazma ucu bilerek acilmaz.
/// <summary>Kosum durumu lookup okumalarinin Application sozlesmesidir.</summary>
public interface ITestRunStatusAppService : IApplicationService
{
    /// <summary>Kimligi verilen kosum durumu satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Kosum durumu lookup gorunumu.</returns>
    Task<TestRunStatusDto> GetAsync(Guid id);

    /// <summary>Kosum durumu satirlarini kararli siralama ve sayfalamayla getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Kosum durumu lookup sayfasi.</returns>
    Task<PagedResultDto<TestRunStatusDto>> GetListAsync(LookupListInput input);
}
