using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Esql.Extensions;
using Microsoft.Extensions.Options;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Search;
using Ptn.DatabaseChecker.Search;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Repository.Search;

// islevi: Elasticsearch index'leri icin generic repository base'idir.
// sistemdeki gorevi: ES|QL predicate okumalarini, bulk upsert/silme operasyonlarini ve Query DSL yardimcilarini tek yerde toplar.
public abstract class ElasticsearchRepository<TDocument> : IElasticsearchRepository<TDocument>
    where TDocument : class, ISearchDocument
{
    // Elasticsearch client instance'i; yalnizca EFCore katmaninda tutulur.
    protected ElasticsearchClient Client { get; }

    // Elasticsearch davranis opsiyonlari; appsettings'ten bind edilir.
    protected ElasticsearchOptions Options { get; }

    protected ElasticsearchRepository(ElasticsearchClient client, IOptions<ElasticsearchOptions> options)
    {
        Client = client;
        Options = options.Value;
    }

    // Predicate'e uyan ilk dokumani ES|QL provider ile getirir.
    public async Task<TDocument?> GetByAsync(Expression<Func<TDocument, bool>> predicate)
    {
        return await Client.Esql.CreateQuery<TDocument>()
            .From(TDocument.IndexName)
            .Where(predicate)
            .AsEsqlQueryable()
            .FirstOrDefaultAsync();
    }

    // Predicate'e uyan dokumanlari ES|QL provider ile getirir.
    public async Task<List<TDocument>> GetListByAsync(Expression<Func<TDocument, bool>> predicate, int maxResultCount)
    {
        var safeMaxResultCount = Math.Max(1, maxResultCount);

        return await Client.Esql.CreateQuery<TDocument>()
            .From(TDocument.IndexName)
            .Where(predicate)
            .Take(safeMaxResultCount)
            .AsEsqlQueryable()
            .ToListAsync();
    }

    // Dokumani upsert eder; basarisizlik false doner ve yazma akisi kirilmaz.
    public async Task<bool> IndexAsync(TDocument document)
    {
        var response = await Client.IndexAsync(document, descriptor => descriptor
            .Index(TDocument.IndexName)
            .Id(document.Id.ToString()));

        return response.IsValidResponse;
    }

    // Dokumanlari tek bulk istegiyle upsert eder; bos liste ES'e istek gondermez.
    public async Task<bool> IndexManyAsync(IReadOnlyCollection<TDocument> documents)
    {
        if (documents.Count == 0)
        {
            return true;
        }

        var response = await Client.BulkAsync(descriptor => descriptor
            .Index(TDocument.IndexName)
            .IndexMany(documents));

        return response.IsValidResponse && !response.Errors;
    }

    // Dokumani kaynak entity Id'si ile siler.
    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await Client.DeleteAsync(TDocument.IndexName, id.ToString());
        return response.IsValidResponse;
    }

    // Query DSL sorgusunu sayfali calistirir ve toplam sayiyi ayni response'tan dondurur.
    protected async Task<PagedResultDto<TDocument>> SearchPagedAsync(Query query, int skipCount, int maxResultCount)
    {
        var safeSkipCount = Math.Max(0, skipCount);
        var safeMaxResultCount = Math.Max(1, maxResultCount);
        var response = await Client.SearchAsync<TDocument>(descriptor => descriptor
            .Indices(TDocument.IndexName)
            .From(safeSkipCount)
            .Size(safeMaxResultCount)
            .Query(query));

        EnsureValid(response);
        return new PagedResultDto<TDocument>(response.Total, response.Documents.ToList());
    }

    // Tum dokumanlari eslestiren sorgu.
    protected static Query MatchAll()
    {
        return new MatchAllQuery();
    }

    // Sorgulardan en az birinin eslesmesini ister.
    protected static Query AnyOf(params Query[] queries)
    {
        return new BoolQuery
        {
            Should = queries,
            MinimumShouldMatch = 1
        };
    }

    // Analiz edilen text alanda fuzzy metin aramasi kurar.
    protected Query FuzzyMatch(Expression<Func<TDocument, object?>> field, string text)
    {
        return new MatchQuery
        {
            Field = Infer.Field(field),
            Query = text,
            Fuzziness = new Fuzziness(Options.Fuzziness)
        };
    }

    // Analiz edilmemis alanda birebir eslesme sorgusu kurar.
    protected static Query ExactMatch(Expression<Func<TDocument, object?>> field, string value)
    {
        return new TermQuery
        {
            Field = Infer.Field(field),
            Value = value
        };
    }

    // Okuma tarafinda ES erisilemezse bos sonuc gibi davranilmaz.
    private static void EnsureValid(SearchResponse<TDocument> response)
    {
        if (!response.IsValidResponse)
        {
            throw new BusinessException(GeneralExceptionCodes.SearchUnavailable);
        }
    }
}
