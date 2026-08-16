using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ptn.ApiContractChecker.Services;

// islevi: Olusturulabilir ana entity servisleri icin create ve bulk-create kontratini tanimlar.
// sistemdeki gorevi: Tekil ve toplu create imzalarinin her servis arayuzunde tekrar yazilmasini engeller.
public interface IEntityCreateAppService<TDto, TCreateDto> : IEntityReadAppService<TDto>
    where TDto : class
    where TCreateDto : class
{
    /// <summary>
    /// Yeni kayit olusturur.
    /// </summary>
    /// <param name="input">Olusturma istegi.</param>
    /// <returns>Olusturulan kaydin API cevap modeli.</returns>
    Task<TDto> CreateAsync(TCreateDto input);

    /// <summary>
    /// Kayitlari toplu olarak olusturur.
    /// </summary>
    /// <param name="inputs">Olusturma istekleri.</param>
    /// <returns>Olusturulan kayitlarin API cevap modelleri.</returns>
    Task<List<TDto>> CreateManyAsync(List<TCreateDto> inputs);
}
