using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Lookups;
using SystemStandards.Results;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Tum lookup controller'larinin 5'li CRUD endpointlerini tek generic tabanda tanimlar.
// sistemdeki gorevi: 8 lookup icin ayni endpoint govdelerinin kopyalanmasini engeller; concrete controller yalnizca [Route] verir (golden rule 1: is bir kez yapilir).
/// <summary>
/// Kararli lookup kaynaklari icin ortak CRUD transport yuzeyi.
/// </summary>
public abstract class LookupControllerBase<TAppService, TDto, TCreateDto, TUpdateDto> : DatabaseCheckerController
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
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.EntityById)]
    [Authorize(DatabaseCheckerPermissions.Lookups.View)]
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
    [Authorize(DatabaseCheckerPermissions.Lookups.View)]
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
    [Authorize(DatabaseCheckerPermissions.Lookups.Manage)]
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
    [HttpPut(DatabaseCheckerHttpApiConstants.Segments.EntityById)]
    [Authorize(DatabaseCheckerPermissions.Lookups.Manage)]
    public virtual async Task<Result<TDto>> Update(Guid id, [FromBody] TUpdateDto input)
    {
        var result = await AppService.UpdateAsync(id, input);
        return result;
    }

    /// <summary>
    /// Lookup satirini siler.
    /// </summary>
    /// <param name="id">Silinecek satirin kimligi.</param>
    /// <returns>Islem sonucu.</returns>
    [HttpDelete(DatabaseCheckerHttpApiConstants.Segments.EntityById)]
    [Authorize(DatabaseCheckerPermissions.Lookups.Manage)]
    public virtual async Task<Result> Delete(Guid id)
    {
        await AppService.DeleteAsync(id);
        return Result.Success();
    }
}
