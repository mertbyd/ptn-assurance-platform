using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services;

// islevi: Tum ana entity servisleri icin ortak get/get-list kontratini tanimlar.
// sistemdeki gorevi: Controller ve diger Application akislarinin okuma imzalarini tek generic kaynaktan almasini saglar.
public interface IEntityReadAppService<TDto> : IApplicationService
    where TDto : class
{
    /// <summary>
    /// Kimlige gore tek kayit getirir.
    /// </summary>
    /// <param name="id">Okunacak kaydin kimligi.</param>
    /// <returns>Kaydin API cevap modeli.</returns>
    Task<TDto> GetAsync(Guid id);

    /// <summary>
    /// Kayitlari sayfali olarak listeler.
    /// </summary>
    /// <param name="input">Sayfalama parametreleri.</param>
    /// <returns>Toplam sayi ve sayfa icerigi.</returns>
    Task<PagedResultDto<TDto>> GetListAsync(PagedResultRequestDto input);
}
