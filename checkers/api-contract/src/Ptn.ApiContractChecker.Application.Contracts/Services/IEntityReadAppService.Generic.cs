using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.ApiContractChecker.Services;

// islevi: Detay ve liste cevaplari ile sayfalama girdisi farkli ana entity okuma kontratini tanimlar.
// sistemdeki gorevi: Agir detay govdesi olan entity'lerin mevcut read tabanini kopyalamadan hafif liste sozlesmesi acmasini saglar.
public interface IEntityReadAppService<TDetailDto, TListDto, in TListInput> : IApplicationService
    where TDetailDto : class
    where TListDto : class
    where TListInput : PagedResultRequestDto
{
    // Kimlige gore tek kaydi detay cevap modeliyle getirir.
    Task<TDetailDto> GetAsync(Guid id);

    // Kayitlari tipli filtre ve ABP sayfalama sozlesmesiyle listeler.
    Task<PagedResultDto<TListDto>> GetListAsync(TListInput input);
}
