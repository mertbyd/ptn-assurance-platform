using System;
using System.Threading.Tasks;
using FluentValidation;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Ptn.DatabaseChecker.Application.Services;

// islevi: Guncellenebilir/silinebilir ana entity servisleri icin update ve delete akislarini create base'ine ekler.
// sistemdeki gorevi: Concrete servislerde update validasyon, manager dogrulamasi ve persist kodunun tekrar yazilmasini engeller.
public abstract class EntityCrudAppServiceBase<TEntity, TReadModel, TDto, TCreateDto, TUpdateDto, TCreateModel, TUpdateModel>
    : EntityCreateAppServiceBase<TEntity, TReadModel, TDto, TCreateDto, TCreateModel>
    where TEntity : class, IEntity<Guid>
    where TReadModel : class
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    protected EntityCrudAppServiceBase(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<TEntity, Guid> repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // Update DTO format validasyonu FluentValidation ile Application.Contracts katmanindan gelir.
    protected IValidator<TUpdateDto> UpdateValidator => LazyGetRequiredService<IValidator<TUpdateDto>>();

    // islevi: Mevcut kaydi update DTO'su ile dogrular, manager kurallarindan gecirir ve kaydedip detayli DTO dondurur.
    public virtual async Task<TDto> UpdateAsync(Guid id, TUpdateDto input)
    {
        await UpdateValidator.ValidateAndThrowAsync(input);
        var entity = await GetEntityForMutationAsync(id);
        var model = MapToUpdateModel(input);
        await ApplyUpdateAsync(entity, model);
        var saved = await Repository.UpdateAsync(entity, autoSave: true);
        return await MapSavedEntityAsync(saved.Id);
    }

    // islevi: Kaydi siler; entity ISoftDelete destekliyorsa ABP global soft-delete davranisi devreye girer.
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetEntityForMutationAsync(id);
        await Repository.DeleteAsync(entity, autoSave: true);
    }

    // islevi: Mutasyon oncesi entity'yi load-or-throw semantigiyle getirir.
    protected virtual async Task<TEntity> GetEntityForMutationAsync(Guid id)
    {
        var entity = await Repository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id);
        }

        return entity;
    }

    // islevi: Manager update dogrulamasi sonrasi Mapperly ile mevcut entity uzerine alanlari uygular.
    protected virtual async Task ApplyUpdateAsync(TEntity entity, TUpdateModel model)
    {
        await ValidateUpdateModelAsync(entity, model);
        MapToEntity(model, entity);
    }

    // islevi: Update DTO'yu domain manager modeline Mapperly ile tasir.
    protected abstract TUpdateModel MapToUpdateModel(TUpdateDto input);

    // islevi: Update modelindeki domain kurallarini manager'a dogrulatir.
    protected abstract Task<TUpdateModel> ValidateUpdateModelAsync(TEntity entity, TUpdateModel model);

    // islevi: Dogrulanmis update modelini entity uzerine Mapperly ile uygular.
    protected abstract void MapToEntity(TUpdateModel model, TEntity entity);
}
