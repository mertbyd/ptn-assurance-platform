using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Services;

// islevi: Tum ana entity servisleri icin ortak get/get-list kontratini tanimlar.
// sistemdeki gorevi: Controller ve diger Application akislarinin okuma imzalarini tek generic kaynaktan almasini saglar.
public interface IEntityReadAppService<TDto>
    : IEntityReadAppService<TDto, TDto, PagedResultRequestDto>
    where TDto : class
{
}
