using System;
using System.Threading.Tasks;

namespace Ptn.ApiContractChecker.Services;

// islevi: Guncellenebilir ve silinebilir ana entity servisleri icin CRUD kontratini tanimlar.
// sistemdeki gorevi: Update/delete imzalarini tekrar etmeden concrete servis arayuzlerine tasir.
public interface IEntityCrudAppService<TDto, TCreateDto, TUpdateDto> : IEntityCreateAppService<TDto, TCreateDto>
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    /// <summary>
    /// Mevcut kaydi gunceller.
    /// </summary>
    /// <param name="id">Guncellenecek kaydin kimligi.</param>
    /// <param name="input">Guncelleme istegi.</param>
    /// <returns>Guncellenen kaydin API cevap modeli.</returns>
    Task<TDto> UpdateAsync(Guid id, TUpdateDto input);

    /// <summary>
    /// Kaydi siler.
    /// </summary>
    /// <param name="id">Silinecek kaydin kimligi.</param>
    Task DeleteAsync(Guid id);
}
