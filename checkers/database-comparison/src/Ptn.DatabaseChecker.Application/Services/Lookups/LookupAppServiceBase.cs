using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Managers.Lookups;
using Ptn.DatabaseChecker.Models.Lookups;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.DatabaseChecker.Application.Services.Lookups;

// islevi: Lookup CRUD dikeyinin manager'a bagli hook'larini (benzersizlik dogrulama, varlik kontrolu, update mutasyonu) tek yerde uygular.
// sistemdeki gorevi: Concrete lookup AppService'lerinde yalnizca Mapperly hook'larini birakir; benzersizlik ve akis mantiginin her lookup icin tekrar yazilmasini engeller (golden rule 1: is bir kez yapilir).
public abstract class LookupAppServiceBase<TEntity, TDto, TCreateDto, TUpdateDto>
    : LookupCrudAppService<TEntity, TDto, TCreateDto, TUpdateDto, LookupCreateModel, LookupUpdateModel>
    where TEntity : LookupEntity
    where TDto : class
    where TCreateDto : LookupCreateDto
    where TUpdateDto : LookupUpdateDto
{
    protected LookupAppServiceBase(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<TEntity, Guid> repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // Bu entity turune ozel generic lookup manager; benzersizlik kurallari buradan yurur.
    protected LookupManager<TEntity> Manager => LazyGetRequiredService<LookupManager<TEntity>>();

    // Varlik kontrolu manager'in ortak load-or-throw yardimcisina devredilir.
    protected override Task<TEntity> EnsureExistsAsync(Guid id) => Manager.EnsureExistsAsync(id);

    // Tekil create'te Code benzersizligini manager dogrular.
    protected override Task<LookupCreateModel> CreateModelAsync(LookupCreateModel model)
        => Manager.ValidateCreateAsync(model);

    // Toplu create'te tekrarlar ve DB benzersizligi tek pasoda manager tarafindan dogrulanir.
    protected override Task<List<LookupCreateModel>> CreateModelsAsync(List<LookupCreateModel> models)
        => Manager.ValidateCreateManyAsync(models);

    // Update: once manager benzersizligi dogrular, sonra Mapperly ile alanlar mevcut entity'ye kopyalanir.
    protected override async Task<TEntity> UpdateEntityAsync(TEntity entity, LookupUpdateModel model)
    {
        await Manager.ValidateUpdateAsync(entity, model);
        ApplyUpdate(model, entity);
        return entity;
    }

    // Mapperly ile update modelini mevcut entity uzerine yazar; concrete AppService kendi mapper'ini baglar.
    protected abstract void ApplyUpdate(LookupUpdateModel model, TEntity entity);
}
