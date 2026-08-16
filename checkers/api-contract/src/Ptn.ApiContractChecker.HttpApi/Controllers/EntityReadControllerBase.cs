using Ptn.ApiContractChecker.Services;
using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Controllers;

// islevi: Ana entity controller'lari icin ortak read endpointlerini tanimlar.
// sistemdeki gorevi: Get/GetList HTTP govdelerini her controller'da tekrar yazmadan servis katmanina ince bir kopru kurar.
public abstract class EntityReadControllerBase<TAppService, TDto>
    : EntityReadControllerBase<TAppService, TDto, TDto, PagedResultRequestDto>
    where TAppService : class, IEntityReadAppService<TDto>
    where TDto : class
{
}
