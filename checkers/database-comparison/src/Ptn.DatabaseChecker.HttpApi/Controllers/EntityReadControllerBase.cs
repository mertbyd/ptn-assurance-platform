using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Services;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Controllers;

// islevi: Ana entity controller'lari icin ortak read endpointlerini tanimlar.
// sistemdeki gorevi: Get/GetList HTTP govdelerini her controller'da tekrar yazmadan servis katmanina ince bir kopru kurar.
/// <summary>
/// Ana entity controller'lari icin ortak sayfali okuma yuzeyi.
/// </summary>
public abstract class EntityReadControllerBase<TAppService, TDto> : DatabaseCheckerController
    where TAppService : class, IEntityReadAppService<TDto>
    where TDto : class
{
    // Modulin AppService'i; controller logic tasimadan lazy cozumlenir.
    protected TAppService AppService => LazyGetRequiredService<TAppService>();

    /// <summary>
    /// Kimlige gore tek kayit getirir.
    /// </summary>
    /// <param name="id">Okunacak kaydin kimligi.</param>
    /// <returns>Kaydin API cevap modeli.</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.EntityById)]
    public virtual async Task<Result<TDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>
    /// Kayitlari sayfali olarak listeler.
    /// </summary>
    /// <param name="input">Sayfalama parametreleri.</param>
    /// <returns>Toplam sayi ve sayfa icerigi.</returns>
    [HttpGet]
    public virtual async Task<Result<PagedResultDto<TDto>>> GetList([FromQuery] PagedResultRequestDto input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }
}
