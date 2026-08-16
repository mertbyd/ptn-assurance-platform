using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services.Lookups;

// islevi: Tum lookup tablolarinin okuma, olusturma, guncelleme ve pasiflestirme akisini tek merkezden yonetir.
// sistemdeki gorevi: Kararli kodu ve gecmis referanslari koruyan lookup yonetim akisinin her tabloda tekrar yazilmasini engeller.
public abstract class LookupCrudAppService<TEntity, TDto, TCreateDto, TUpdateDto, TCreateModel, TUpdateModel>
    : ApiContractCheckerAppService
    where TEntity : class, IEntity<Guid>
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    protected readonly IRepository<TEntity, Guid> Repository;

    protected LookupCrudAppService(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<TEntity, Guid> repository)
        : base(abpLazyServiceProvider)
    {
        Repository = repository;
    }

    protected IValidator<TCreateDto> CreateValidator => LazyGetRequiredService<IValidator<TCreateDto>>();
    protected IValidator<TUpdateDto> UpdateValidator => LazyGetRequiredService<IValidator<TUpdateDto>>();

    protected abstract Task<TEntity> EnsureExistsAsync(Guid id);
    protected abstract Task<TCreateModel> CreateModelAsync(TCreateModel model);
    protected abstract Task<List<TCreateModel>> CreateModelsAsync(List<TCreateModel> models);
    protected abstract TEntity CreateEntity(TCreateModel model);
    // Degisebilir update modelini mevcut lookup entity'sine uygular; kararli Code modelde bulunmaz.
    protected abstract TEntity UpdateEntity(TEntity entity, TUpdateModel model);

    // Lookup entity'sini domain/manager davranisi uzerinden fiziksel silmeden pasife ceker.
    protected abstract TEntity PassivateEntity(TEntity entity);
    protected abstract TDto MapToDto(TEntity entity);
    protected abstract List<TDto> MapToDto(List<TEntity> entities);
    protected abstract TCreateModel MapToCreateModel(TCreateDto input);
    protected abstract TUpdateModel MapToUpdateModel(TUpdateDto input);

    public virtual async Task<TDto> GetAsync(Guid id)
    {
        var entity = await EnsureExistsAsync(id);
        return MapToDto(entity);
    }

    public virtual async Task<PagedResultDto<TDto>> GetListAsync(PagedResultRequestDto input)
    {
        var totalCount = await Repository.GetCountAsync();
        var entities = await Repository.GetPagedListAsync(input.SkipCount, input.MaxResultCount, sorting: string.Empty);
        return new PagedResultDto<TDto>(totalCount, MapToDto(entities));
    }

    public virtual async Task<TDto> CreateAsync(TCreateDto input)
    {
        await CreateValidator.ValidateAndThrowAsync(input);
        var model = MapToCreateModel(input);
        var entity = await CreateEntityAsync(model);
        var inserted = await Repository.InsertAsync(entity, autoSave: true);
        return MapToDto(inserted);
    }

    public virtual async Task<List<TDto>> CreateManyAsync(List<TCreateDto> inputs)
    {
        var models = new List<TCreateModel>();
        foreach (var dto in inputs)
        {
            await CreateValidator.ValidateAndThrowAsync(dto);
            models.Add(MapToCreateModel(dto));
        }

        var entities = await CreateEntitiesAsync(models);
        await Repository.InsertManyAsync(entities, autoSave: true);
        return MapToDto(entities);
    }

    public virtual async Task<TDto> UpdateAsync(Guid id, TUpdateDto input)
    {
        await UpdateValidator.ValidateAndThrowAsync(input);
        var existing = await EnsureExistsAsync(id);
        var model = MapToUpdateModel(input);
        var updated = UpdateEntity(existing, model);
        var saved = await Repository.UpdateAsync(updated, autoSave: true);
        return MapToDto(saved);
    }

    // Lookup satirini IPassivable yasam dongusuyle emekli eder ve global filtreye takilmadan yuklu entity'den cevap uretir.
    public virtual async Task<TDto> PassivateAsync(Guid id)
    {
        var existing = await EnsureExistsAsync(id);
        var passivated = PassivateEntity(existing);
        var saved = await Repository.UpdateAsync(passivated, autoSave: true);
        return MapToDto(saved);
    }

    protected virtual async Task<TEntity> CreateEntityAsync(TCreateModel model)
    {
        var validatedModel = await CreateModelAsync(model);
        return CreateEntity(validatedModel);
    }

    protected virtual async Task<List<TEntity>> CreateEntitiesAsync(List<TCreateModel> models)
    {
        var validatedModels = await CreateModelsAsync(models);
        var entities = new List<TEntity>(validatedModels.Count);
        foreach (var model in validatedModels)
        {
            entities.Add(CreateEntity(model));
        }

        return entities;
    }
}
