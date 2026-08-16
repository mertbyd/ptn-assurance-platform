using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Lookups;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Controllers.Lookups;

// islevi: Tum lookup controller'larinin besli okuma/yonetim endpoint setini tek generic tabanda tanimlar.
// sistemdeki gorevi: Lookup tablolarinda fiziksel silme acmadan ayni endpoint govdelerinin kopyalanmasini engeller; concrete controller yalniz rota verir.
public abstract class LookupControllerBase<TAppService, TDto, TCreateDto, TUpdateDto> : ApiContractCheckerController
    where TAppService : class, ILookupAppService<TDto, TCreateDto, TUpdateDto>
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    // Dikeyin AppService'i; lazy cozumlenir, controller ince kalir.
    protected TAppService AppService => LazyGetRequiredService<TAppService>();

    /// <summary>
    /// Kimlige gore tek lookup satiri getirir.
    /// </summary>
    /// <param name="id">Lookup satirinin kimligi.</param>
    /// <returns>Lookup detayi.</returns>
    [HttpGet(ApiContractCheckerRoutes.EntityById)]
    [Authorize(ApiContractCheckerPermissions.Lookups.View)]
    public virtual async Task<Result<TDto>> Get(Guid id)
    {
        var result = await AppService.GetAsync(id);
        return result;
    }

    /// <summary>
    /// Lookup satirlarini sayfali olarak listeler.
    /// </summary>
    /// <param name="input">Sayfalama parametreleri.</param>
    /// <returns>Toplam sayi ve sayfa icerigi.</returns>
    [HttpGet]
    [Authorize(ApiContractCheckerPermissions.Lookups.View)]
    public virtual async Task<Result<PagedResultDto<TDto>>> GetList([FromQuery] PagedResultRequestDto input)
    {
        var result = await AppService.GetListAsync(input);
        return result;
    }

    /// <summary>
    /// Yeni lookup satiri olusturur.
    /// </summary>
    /// <param name="input">Olusturma istegi.</param>
    /// <returns>Olusturulan satirin detayi.</returns>
    [HttpPost]
    [Authorize(ApiContractCheckerPermissions.Lookups.Manage)]
    public virtual async Task<Result<TDto>> Create([FromBody] TCreateDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }

    /// <summary>
    /// Mevcut lookup satirini gunceller.
    /// </summary>
    /// <param name="id">Guncellenecek satirin kimligi.</param>
    /// <param name="input">Guncelleme istegi.</param>
    /// <returns>Guncellenen satirin detayi.</returns>
    [HttpPut(ApiContractCheckerRoutes.EntityById)]
    [Authorize(ApiContractCheckerPermissions.Lookups.Manage)]
    public virtual async Task<Result<TDto>> Update(Guid id, [FromBody] TUpdateDto input)
    {
        var result = await AppService.UpdateAsync(id, input);
        return result;
    }

    /// <summary>
    /// Lookup satirini fiziksel olarak silmeden pasife ceker.
    /// </summary>
    /// <param name="id">Silinecek satirin kimligi.</param>
    /// <returns>Pasiflestirilen lookup satirinin detayi.</returns>
    [HttpPost(ApiContractCheckerRoutes.LookupPassivate)]
    [Authorize(ApiContractCheckerPermissions.Lookups.Manage)]
    public virtual async Task<Result<TDto>> Passivate(Guid id)
    {
        var result = await AppService.PassivateAsync(id);
        return result;
    }
}
