using System;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Dtos.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Lookups;

// islevi: Test hukmu lookup'inin salt-okunur use-case'lerini tanimlar.
// sistemdeki gorevi: Satirlar seed ile geldigi icin sozlesme yalniz okuma tasir; yazma ucu bilerek acilmaz.
/// <summary>Test hukmu lookup okumalarinin Application sozlesmesidir.</summary>
public interface ITestOutcomeStatusAppService : IApplicationService
{
    /// <summary>Kimligi verilen test hukmu satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Build politikasini da tasiyan test hukmu gorunumu.</returns>
    Task<TestOutcomeStatusDto> GetAsync(Guid id);

    /// <summary>Test hukmu satirlarini kararli siralama ve sayfalamayla getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Test hukmu lookup sayfasi.</returns>
    Task<PagedResultDto<TestOutcomeStatusDto>> GetListAsync(LookupListInput input);
}
