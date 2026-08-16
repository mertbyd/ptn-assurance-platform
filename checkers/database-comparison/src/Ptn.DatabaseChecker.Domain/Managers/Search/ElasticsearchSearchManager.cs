using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface;
using Ptn.DatabaseChecker.Interface.Search;
using Ptn.DatabaseChecker.Search;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;

namespace Ptn.DatabaseChecker.Managers.Search;

// islevi: Arama index'i olan entity'lerin reindex bakim akisini generic olarak isletir.
// sistemdeki gorevi: Sayfali DB okuma, Mapperly delegate ile dokuman uretme ve bulk ES yazma davranisi her entity'de tekrar yazilmaz.
public abstract class ElasticsearchSearchManager<TEntity, TDocument> : BaseManager<TEntity>
    where TEntity : class, IEntity<Guid>
    where TDocument : class, ISearchDocument
{
    // Elasticsearch typed options degerleri lazy olarak okunur.
    private ElasticsearchOptions Options => LazyGetRequiredService<IOptions<ElasticsearchOptions>>().Value;

    // Tureyen manager kendi search repository'sini bu hook ile verir.
    protected abstract IElasticsearchRepository<TDocument> SearchRepository { get; }

    protected ElasticsearchSearchManager(
        IBaseRepository<TEntity> repository,
        IAbpLazyServiceProvider abpLazyServiceProvider)
        : base(repository, abpLazyServiceProvider)
    {
    }

    // Kaynak tabloyu sayfali okuyup index'i gunceller; entity -> dokuman map'i Application Mapperly delegate'inden gelir.
    public async Task<long> ReindexAsync(Func<List<TEntity>, List<TDocument>> mapToDocuments)
    {
        long totalIndexed = 0;
        var skipCount = 0;
        var batchSize = Math.Max(1, Options.ReindexBatchSize);

        while (true)
        {
            var entities = await Repository.GetPagedListAsync(
                skipCount,
                batchSize,
                nameof(IEntity<Guid>.Id),
                includeDetails: true);

            if (entities.Count == 0)
            {
                break;
            }

            var documents = mapToDocuments(entities);
            if (!await SearchRepository.IndexManyAsync(documents))
            {
                throw new BusinessException(GeneralExceptionCodes.SearchUnavailable);
            }

            totalIndexed += documents.Count;
            skipCount += entities.Count;

            if (entities.Count < batchSize)
            {
                break;
            }
        }

        return totalIndexed;
    }
}
