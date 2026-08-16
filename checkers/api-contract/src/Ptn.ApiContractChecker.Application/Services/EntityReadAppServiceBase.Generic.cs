using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Ptn.ApiContractChecker.Application.Services;

// islevi: Detay ve liste read-model/DTO tipleri ile filtre girdisi farkli entity okuma akislarini yonetir.
// sistemdeki gorevi: Agir detay govdesi olan entity'lerde ABP sayfalama ve bulunamadi kontrolunu mevcut tabanla ortaklastirir.
public abstract class EntityReadAppServiceBase<
    TEntity,
    TDetailReadModel,
    TListReadModel,
    TDetailDto,
    TListDto,
    TListInput> : ApiContractCheckerAppService
    where TEntity : class, IEntity<Guid>
    where TDetailReadModel : class
    where TListReadModel : class
    where TDetailDto : class
    where TListDto : class
    where TListInput : PagedResultRequestDto
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

    // Kimlige gore tek kaydi detay read-model ve DTO'suyla getirir.
    public virtual async Task<TDetailDto> GetAsync(Guid id)
    {
        var readModel = await GetReadModelAsync(id);
        return MapToDetailDto(readModel);
    }

    // Kayitlari tipli filtre ve ABP sayfalama sozlesmesiyle hafif liste DTO'larina cevirir.
    public virtual async Task<PagedResultDto<TListDto>> GetListAsync(TListInput input)
    {
        var totalCount = await GetTotalCountAsync(input);
        var readModels = await GetPagedReadModelsAsync(input);
        return new PagedResultDto<TListDto>(totalCount, MapToListDto(readModels));
    }

    // Entity bulunamazsa ABP'nin standart EntityNotFoundException davranisini uretir.
    protected static TReadModel EnsureFound<TReadModel>(TReadModel? readModel, Guid id)
        where TReadModel : class
    {
        if (readModel == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id);
        }

        return readModel;
    }

    // Default tekil okuma; detayli repository gereken servisler override eder.
    protected virtual async Task<TDetailReadModel> GetReadModelAsync(Guid id)
    {
        var entity = await Repository.FindAsync(id);
        return EnsureFound(entity as TDetailReadModel, id);
    }

    // Default sayfali okuma; hafif projeksiyon gereken servisler override eder.
    protected virtual async Task<List<TListReadModel>> GetPagedReadModelsAsync(TListInput input)
    {
        var entities = await Repository.GetPagedListAsync(
            input.SkipCount,
            input.MaxResultCount,
            sorting: string.Empty);
        return entities.Cast<TListReadModel>().ToList();
    }

    // Default toplam sayi okuma; filtreli servisler override eder.
    protected virtual Task<long> GetTotalCountAsync(TListInput input)
    {
        return Repository.GetCountAsync();
    }

    // Entity kaydedildikten sonra detay read-model listesini tek sorguyla geri getirir.
    protected virtual async Task<List<TDetailReadModel>> GetReadModelsByIdsAsync(List<Guid> ids)
    {
        var entities = await Repository.GetListAsync(entity => ids.Contains(entity.Id));
        return entities.Cast<TDetailReadModel>().ToList();
    }

    // Tek detay read-model'ini detay API DTO'suna Mapperly ile tasir.
    protected abstract TDetailDto MapToDetailDto(TDetailReadModel readModel);

    // Hafif liste read-model'lerini liste API DTO'larina Mapperly ile tasir.
    protected abstract List<TListDto> MapToListDto(List<TListReadModel> readModels);
}
