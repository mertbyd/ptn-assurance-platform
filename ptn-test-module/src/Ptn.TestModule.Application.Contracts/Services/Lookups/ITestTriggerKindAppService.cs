using System;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Dtos.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Lookups;

// islevi: Tetikleme turu lookup'inin salt-okunur use-case'lerini tanimlar.
// sistemdeki gorevi: Satirlar seed ile geldigi icin sozlesme yalniz okuma tasir; yazma ucu bilerek acilmaz.
/// <summary>Tetikleme turu lookup okumalarinin Application sozlesmesidir.</summary>
public interface ITestTriggerKindAppService : IApplicationService
{
    /// <summary>Kimligi verilen tetikleme turu satirini getirir.</summary>
    /// <param name="id">Okunacak lookup satiri kimligi.</param>
    /// <returns>Tetikleme turu lookup gorunumu.</returns>
    Task<TestTriggerKindDto> GetAsync(Guid id);

    /// <summary>Tetikleme turu satirlarini kararli siralama ve sayfalamayla getirir.</summary>
    /// <param name="input">Sayfalama girdisi.</param>
    /// <returns>Tetikleme turu lookup sayfasi.</returns>
    Task<PagedResultDto<TestTriggerKindDto>> GetListAsync(LookupListInput input);
}
