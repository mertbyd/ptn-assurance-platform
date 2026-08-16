using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Ptn.DatabaseChecker.Application.Services;

// islevi: Ana entity servisleri icin get/get-list akislarini tek merkezden yonetir.
// sistemdeki gorevi: Concrete AppService'lerde okuma sayfalama ve entity bulunamadi kontrolunun tekrar yazilmasini engeller.
public abstract class EntityReadAppServiceBase<TEntity, TReadModel, TDto> : DatabaseCheckerAppService
    where TEntity : class, IEntity<Guid>
    where TReadModel : class
    where TDto : class
{
    // Repository ABP altyapisindan gelir; servislerde manual DI kaydi gerektirmez.
    protected readonly IRepository<TEntity, Guid> Repository;

    protected EntityReadAppServiceBase(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<TEntity, Guid> repository)
        : base(abpLazyServiceProvider)
    {
        Repository = repository;
    }

    // islevi: Kimlige gore tek kaydi detay okuma modeliyle getirir.
    public virtual async Task<TDto> GetAsync(Guid id)
    {
        var readModel = await GetReadModelAsync(id);
        return MapToDto(readModel);
    }

    // islevi: Kayitlari sayfali olarak detay okuma modeliyle getirir.
    public virtual async Task<PagedResultDto<TDto>> GetListAsync(PagedResultRequestDto input)
    {
        var totalCount = await GetTotalCountAsync(input);
        var readModels = await GetPagedReadModelsAsync(input);
        return new PagedResultDto<TDto>(totalCount, MapToDto(readModels));
    }

    // islevi: Entity bulunamazsa ABP'nin standart EntityNotFoundException davranisini uretir.
    protected static TReadModel EnsureFound(TReadModel? readModel, Guid id)
    {
        if (readModel == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id);
        }

        return readModel;
    }

    // islevi: Default tekil okuma; detayli repository gereken servisler override eder.
    protected virtual async Task<TReadModel> GetReadModelAsync(Guid id)
    {
        var entity = await Repository.FindAsync(id);
        return EnsureFound(CastEntityReadModel(entity), id);
    }

    // islevi: Default sayfali okuma; detayli repository gereken servisler override eder.
    protected virtual async Task<List<TReadModel>> GetPagedReadModelsAsync(PagedResultRequestDto input)
    {
        var entities = await Repository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, sorting: string.Empty);
        return CastEntityReadModels(entities);
    }

    // islevi: Default toplam sayi okuma; filtreli servisler override eder.
    protected virtual Task<long> GetTotalCountAsync(PagedResultRequestDto input)
    {
        return Repository.GetCountAsync();
    }

    // islevi: Entity kaydedildikten sonra detayli read-model listesini tek sorguyla geri getirir.
    protected virtual async Task<List<TReadModel>> GetReadModelsByIdsAsync(List<Guid> ids)
    {
        var entities = await Repository.GetListAsync(x => ids.Contains(x.Id));
        return CastEntityReadModels(entities);
    }

    // islevi: Tek read-model'i API cevap DTO'suna Mapperly ile tasir.
    protected abstract TDto MapToDto(TReadModel readModel);

    // islevi: Read-model listesini API cevap DTO listesine Mapperly ile tasir.
    protected abstract List<TDto> MapToDto(List<TReadModel> readModels);

    // islevi: Entity read modeliyle ayni tipteyse default cast saglar.
    private static TReadModel? CastEntityReadModel(TEntity? entity)
    {
        return entity as TReadModel;
    }

    // islevi: Entity listesi read modeliyle ayni tipteyse default cast saglar.
    private static List<TReadModel> CastEntityReadModels(List<TEntity> entities)
    {
        return entities.Cast<TReadModel>().ToList();
    }
}
