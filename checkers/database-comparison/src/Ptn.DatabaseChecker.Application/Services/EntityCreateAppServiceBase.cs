using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Ptn.DatabaseChecker.Application.Services;

// islevi: Olusturulabilir ana entity servisleri icin create ve bulk-create akislarini tek merkezden yonetir.
// sistemdeki gorevi: DTO validasyon, model mapping, manager dogrulamasi, entity kurulumu ve persist adimlarinin tekrar yazilmasini engeller.
public abstract class EntityCreateAppServiceBase<TEntity, TReadModel, TDto, TCreateDto, TCreateModel>
    : EntityReadAppServiceBase<TEntity, TReadModel, TDto>
    where TEntity : class, IEntity<Guid>
    where TReadModel : class
    where TDto : class
    where TCreateDto : class
{
    protected EntityCreateAppServiceBase(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<TEntity, Guid> repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // Create DTO format validasyonu FluentValidation ile Application.Contracts katmanindan gelir.
    protected IValidator<TCreateDto> CreateValidator => LazyGetRequiredService<IValidator<TCreateDto>>();

    // islevi: Tek kayit olusturma akisinda validasyon, manager dogrulamasi, persist ve detayli DTO donusunu koordine eder.
    public virtual async Task<TDto> CreateAsync(TCreateDto input)
    {
        await CreateValidator.ValidateAndThrowAsync(input);
        var model = MapToCreateModel(input);
        var entity = await CreateEntityAsync(model);
        var inserted = await Repository.InsertAsync(entity, autoSave: true);
        return await MapSavedEntityAsync(inserted.Id);
    }

    // islevi: Toplu kayit olusturma akisinda DTO'lari memory'de hazirlar, manager'a batch dogrulatir ve tek bulk insert yapar.
    public virtual async Task<List<TDto>> CreateManyAsync(List<TCreateDto> inputs)
    {
        if (inputs.Count == 0)
        {
            return new List<TDto>();
        }

        var models = await ValidateAndMapCreateInputsAsync(inputs);
        var entities = await CreateEntitiesAsync(models);
        await Repository.InsertManyAsync(entities, autoSave: true);

        return await MapSavedEntitiesAsync(entities.Select(x => x.Id).ToList());
    }

    // islevi: Create DTO listesini validator ve Mapperly ile domain model listesine cevirir.
    protected async Task<List<TCreateModel>> ValidateAndMapCreateInputsAsync(List<TCreateDto> inputs)
    {
        var models = new List<TCreateModel>(inputs.Count);
        foreach (var input in inputs)
        {
            await CreateValidator.ValidateAndThrowAsync(input);
            models.Add(MapToCreateModel(input));
        }

        return models;
    }

    // islevi: Tek create modelini manager dogrulamasindan gecirip yeni entity uzerine uygular.
    protected virtual async Task<TEntity> CreateEntityAsync(TCreateModel model)
    {
        var validatedModel = await ValidateCreateModelAsync(model);
        return BuildEntity(validatedModel);
    }

    // islevi: Toplu create modellerini manager batch dogrulamasindan gecirip entity listesine cevirir.
    protected virtual async Task<List<TEntity>> CreateEntitiesAsync(List<TCreateModel> models)
    {
        var validatedModels = await ValidateCreateModelsAsync(models);
        return validatedModels.Select(BuildEntity).ToList();
    }

    // islevi: Dogrulanmis create modelinden yeni kimlikli entity kurar ve Mapperly ile alanlari uygular.
    protected TEntity BuildEntity(TCreateModel model)
    {
        var entity = CreateEmptyEntity();
        MapToEntity(model, entity);
        return entity;
    }

    // islevi: Kayit sonrasi tek entity'yi detayli okuyup DTO'ya cevirir.
    protected async Task<TDto> MapSavedEntityAsync(Guid id)
    {
        var readModel = await GetReadModelAsync(id);
        return MapToDto(readModel);
    }

    // islevi: Kayit sonrasi entity listesini tek detayli okuma ile DTO listesine cevirir.
    protected async Task<List<TDto>> MapSavedEntitiesAsync(List<Guid> ids)
    {
        var readModels = await GetReadModelsByIdsAsync(ids);
        return MapToDto(readModels);
    }

    // islevi: Create DTO'yu domain manager modeline Mapperly ile tasir.
    protected abstract TCreateModel MapToCreateModel(TCreateDto input);

    // islevi: Tek create modelindeki domain kurallarini manager'a dogrulatir.
    protected abstract Task<TCreateModel> ValidateCreateModelAsync(TCreateModel model);

    // islevi: Toplu create modelindeki domain kurallarini manager'a batch olarak dogrulatir.
    protected abstract Task<List<TCreateModel>> ValidateCreateModelsAsync(List<TCreateModel> models);

    // islevi: ABP GuidGenerator ile bos entity olusturur.
    protected abstract TEntity CreateEmptyEntity();

    // islevi: Dogrulanmis create modelini entity uzerine Mapperly ile uygular.
    protected abstract void MapToEntity(TCreateModel model, TEntity entity);
}
