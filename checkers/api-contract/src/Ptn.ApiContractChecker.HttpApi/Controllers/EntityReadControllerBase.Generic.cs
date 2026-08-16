using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Services;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Controllers;

// islevi: Detay ve liste DTO'lari ile filtre girdisi farkli controller'larin ortak GET rotalarini tanimlar.
// sistemdeki gorevi: Agir detay govdeli entity'lerin tipli query filtresini rota govdesi kopyalamadan AppService'e aktarir.
public abstract class EntityReadControllerBase<TAppService, TDetailDto, TListDto, TListInput>
    : ApiContractCheckerController
    where TAppService : class, IEntityReadAppService<TDetailDto, TListDto, TListInput>
    where TDetailDto : class
    where TListDto : class
    where TListInput : PagedResultRequestDto
{
    // Modulin AppService'i; controller logic tasimadan lazy cozumlenir.
    protected TAppService AppService => LazyGetRequiredService<TAppService>();

    /// <summary>Kimlige gore tek kayit getirir.</summary>
    [HttpGet(ApiContractCheckerRoutes.EntityById)]
    public virtual async Task<Result<TDetailDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>Kayitlari tipli filtrelerle sayfali olarak listeler.</summary>
    [HttpGet]
    public virtual async Task<Result<PagedResultDto<TListDto>>> GetList([FromQuery] TListInput input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }
}
