using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Search;

namespace Ptn.DatabaseChecker.Interface.Search;

// islevi: Elasticsearch index'leri icin generic okuma/yazma repository kontratini tanimlar.
// sistemdeki gorevi: ES client kullanimini EFCore repository implementasyonlarinda toplar; Domain yalnizca compile-time guvenli sozlesmeyi bilir.
public interface IElasticsearchRepository<TDocument>
    where TDocument : class, ISearchDocument
{
    // Predicate'e uyan ilk dokumani getirir; eslesme yoksa null doner.
    Task<TDocument?> GetByAsync(Expression<Func<TDocument, bool>> predicate);

    // Predicate'e uyan dokumanlari maksimum sonuc sayisiyla getirir.
    Task<List<TDocument>> GetListByAsync(Expression<Func<TDocument, bool>> predicate, int maxResultCount);

    // Dokumani kaynak Id ile upsert eder.
    Task<bool> IndexAsync(TDocument document);

    // Dokumanlari tek bulk istegiyle upsert eder.
    Task<bool> IndexManyAsync(IReadOnlyCollection<TDocument> documents);

    // Dokumani kaynak entity Id'si ile index'ten siler.
    Task<bool> DeleteAsync(Guid id);
}
