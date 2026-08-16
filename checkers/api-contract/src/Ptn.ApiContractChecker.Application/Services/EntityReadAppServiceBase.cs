using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services;

// islevi: Ana entity servisleri icin get/get-list akislarini tek merkezden yonetir.
// sistemdeki gorevi: Concrete AppService'lerde okuma sayfalama ve entity bulunamadi kontrolunun tekrar yazilmasini engeller.
public abstract class EntityReadAppServiceBase<TEntity, TReadModel, TDto>
    : EntityReadAppServiceBase<TEntity, TReadModel, TReadModel, TDto, TDto, PagedResultRequestDto>
    where TEntity : class, IEntity<Guid>
    where TReadModel : class
    where TDto : class
{
    protected EntityReadAppServiceBase(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<TEntity, Guid> repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // islevi: Tek read-model'i API cevap DTO'suna Mapperly ile tasir.
    protected abstract TDto MapToDto(TReadModel readModel);

    // islevi: Read-model listesini API cevap DTO listesine Mapperly ile tasir.
    protected abstract List<TDto> MapToDto(List<TReadModel> readModels);

    // Mevcut kardes servislerin tekil Mapperly hook'unu generic detay hook'una uyarlar.
    protected sealed override TDto MapToDetailDto(TReadModel readModel)
    {
        return MapToDto(readModel);
    }

    // Mevcut kardes servislerin liste Mapperly hook'unu generic liste hook'una uyarlar.
    protected sealed override List<TDto> MapToListDto(List<TReadModel> readModels)
    {
        return MapToDto(readModels);
    }
}
