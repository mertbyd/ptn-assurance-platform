using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Managers.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services.Lookups;

// islevi: Lookup yonetim dikeyinin manager'a bagli varlik, benzersizlik ve pasiflestirme hook'larini tek yerde uygular.
// sistemdeki gorevi: Concrete lookup AppService'lerinde yalniz Mapperly ve entity kurulum hook'larini birakir; ortak yasam dongusunu tekrar ettirmez.
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

    // Code update modelinde bulunmaz; manager gorunen alanlari entity davranisiyla uygular.
    protected override TEntity UpdateEntity(TEntity entity, LookupUpdateModel model)
    {
        Manager.Update(entity, model);
        return entity;
    }

    // Fiziksel silme yerine manager'in ortak IPassivable davranisini uygular.
    protected override TEntity PassivateEntity(TEntity entity)
    {
        Manager.Passivate(entity);
        return entity;
    }
}
