using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.ApiContractChecker.Services.Lookups;

// islevi: Tum lookup AppService'lerinin okuma, olusturma, guncelleme ve pasiflestirme kontratini tek generic arayuzde tanimlar.
// sistemdeki gorevi: Kararli kodlari fiziksel silmeden koruyan ayni bes operasyonun her lookup icin tekrar yazilmasini engeller.
public interface ILookupAppService<TDto, in TCreateDto, in TUpdateDto> : IApplicationService
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    /// <summary>
    /// Kimlige gore tek lookup satiri getirir.
    /// </summary>
    /// <param name="id">Lookup satirinin kimligi.</param>
    /// <returns>Lookup detayi.</returns>
    Task<TDto> GetAsync(Guid id);

    /// <summary>
    /// Lookup satirlarini sayfali olarak listeler.
    /// </summary>
    /// <param name="input">Sayfalama parametreleri.</param>
    /// <returns>Toplam sayi ve sayfa icerigi.</returns>
    Task<PagedResultDto<TDto>> GetListAsync(PagedResultRequestDto input);

    /// <summary>
    /// Yeni lookup satiri olusturur.
    /// </summary>
    /// <param name="input">Olusturma istegi.</param>
    /// <returns>Olusturulan satirin detayi.</returns>
    Task<TDto> CreateAsync(TCreateDto input);

    /// <summary>
    /// Mevcut lookup satirini gunceller.
    /// </summary>
    /// <param name="id">Guncellenecek satirin kimligi.</param>
    /// <param name="input">Guncelleme istegi.</param>
    /// <returns>Guncellenen satirin detayi.</returns>
    Task<TDto> UpdateAsync(Guid id, TUpdateDto input);

    /// <summary>
    /// Lookup satirini fiziksel olarak silmeden pasife ceker.
    /// </summary>
    /// <param name="id">Silinecek satirin kimligi.</param>
    /// <returns>Pasiflestirilen lookup satirinin detayi.</returns>
    Task<TDto> PassivateAsync(Guid id);
}
