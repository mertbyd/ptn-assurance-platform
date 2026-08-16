using System;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Interface;
using Ptn.DatabaseChecker.Interface.Search;
using Ptn.DatabaseChecker.Search;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
namespace Ptn.DatabaseChecker.Application.EventHandlers.Search;
// islevi: Search index'i olan entity'lerin create/update/delete local event'lerini generic olarak isler.
// sistemdeki gorevi: Yeni bir searchable entity sadece mapper hook'unu verir; indexleme event akisi tekrar yazilmaz.
public abstract class ElasticsearchIndexingEventHandler<TEntity, TDocument> :
    ILocalEventHandler<EntityCreatedEventData<TEntity>>,
    ILocalEventHandler<EntityUpdatedEventData<TEntity>>,
    ILocalEventHandler<EntityDeletedEventData<TEntity>>,
    ITransientDependency
    where TEntity : class, IEntity<Guid>
    where TDocument : class, ISearchDocument
{
    // Kaynak entity repository'si; update/create event'lerinde detayli projection icin kullanilir.
    private readonly IBaseRepository<TEntity> _repository;
    // Hedef Elasticsearch repository'si; ES client'a dogrudan bu katman dokunmaz.
    private readonly IElasticsearchRepository<TDocument> _searchRepository;
    protected ElasticsearchIndexingEventHandler(
        IBaseRepository<TEntity> repository,
        IElasticsearchRepository<TDocument> searchRepository)
    {
        _repository = repository;
        _searchRepository = searchRepository;
    }
    // Entity -> index dokumani donusumu Mapperly mapper ile tureyen handler'da yapilir.
    protected abstract TDocument MapToDocument(TEntity entity);
    public async Task HandleEventAsync(EntityCreatedEventData<TEntity> eventData)
    {
        await IndexAsync(eventData.Entity.Id);
    }
    public async Task HandleEventAsync(EntityUpdatedEventData<TEntity> eventData)
    {
        await IndexAsync(eventData.Entity.Id);
    }
    public async Task HandleEventAsync(EntityDeletedEventData<TEntity> eventData)
    {
        await _searchRepository.DeleteAsync(eventData.Entity.Id);
    }
    // Navigation/detail alanlari dolu dokuman uretebilmek icin entity yeniden detayli okunur.
    private async Task IndexAsync(Guid entityId)
    {
        var entity = await _repository.FindAsync(entityId, includeDetails: true);
        if (entity == null)
        {
            return;
        }
        await _searchRepository.IndexAsync(MapToDocument(entity));
    }
}
