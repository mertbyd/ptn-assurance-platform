using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Models.Runs;

namespace Ptn.DatabaseChecker.Interface.Runs;

// islevi: ComparisonRun icin hem hafif header projeksiyonu hem tam-detay okuma sorgularinin kontratini tanimlar.
// sistemdeki gorevi: Liste/get header'i owned jsonb (ScopeSnapshot/Findings/Reports) cekmeden projeksiyonla beslenir; detay okumasi ise tam entity'yi navigation'lariyla getirir.
public interface IComparisonRunRepository : IBaseRepository<ComparisonRun>
{
    // Detay: tek run'i tum navigation'lari VE owned bulgular/kapsam/rapor kolonlariyla getirir; bulunamazsa null.
    Task<ComparisonRun?> FindWithDetailsAsync(Guid id);

    // Header: tek run'i yalnizca header kolonlari + companion adlarla projekte eder; bulunamazsa null.
    Task<ComparisonRunHeaderModel?> FindHeaderAsync(Guid id);

    // Header: run'lari (istege bagli tarif filtresiyle) en yeniden eskiye sayfali, yalnizca header projeksiyonuyla listeler (jsonb cekmez).
    Task<List<ComparisonRunHeaderModel>> GetPagedHeadersAsync(Guid? comparisonDefinitionId, int skipCount, int maxResultCount);

    // Header: verilen run kimliklerini tek sorguda header projeksiyonuyla getirir (kayit sonrasi cevap icin).
    Task<List<ComparisonRunHeaderModel>> GetHeadersByIdsAsync(List<Guid> ids);

    // Filtreye gore toplam run sayisini dondurur (sayfalama toplami).
    Task<long> GetCountByDefinitionAsync(Guid? comparisonDefinitionId);

    /// <summary>
    /// Owned JSON bulgularini sunucuda projekte eder, filtreler ve sayfalar.
    /// </summary>
    Task<FindingPageModel> GetFindingsAsync(
        Guid id,
        FindingQueryModel input,
        CancellationToken cancellationToken = default)
        => Task.FromException<FindingPageModel>(new NotSupportedException());

    /// <summary>
    /// Ayni definition icindeki bir onceki Completed run'in fingerprintlerini okur; boyle bir run yoksa null doner.
    /// </summary>
    Task<IReadOnlyCollection<string>?> FindPreviousCompletedFindingFingerprintsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Referans run'in gorulebilir, daha eski, Completed ve mevcut run ile ayni definition'a ait oldugunu sorgular.
    /// </summary>
    Task<bool> IsValidCompletedReferenceRunAsync(
        Guid currentRunId,
        Guid referenceRunId,
        CancellationToken cancellationToken = default);
}
