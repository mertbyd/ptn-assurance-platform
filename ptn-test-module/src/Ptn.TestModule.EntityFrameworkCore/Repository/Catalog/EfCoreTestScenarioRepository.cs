using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexum.Abp.Foundation.EntityFrameworkCore.Repositories;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace Ptn.TestModule.EntityFrameworkCore.Repository.Catalog;

// islevi: Senaryo katalog sorgularinin EF Core uygulamasini saglar.
// sistemdeki gorevi: Son surum, yayinlanmis surum ve version uretimi LINQ'ini provider katmaninda tutar.
[ExposeServices(typeof(ITestScenarioRepository))]
public class EfCoreTestScenarioRepository
    : BaseEfCoreRepository<TestModuleDbContext, TestScenario, Guid>, ITestScenarioRepository
{
    // Capraz tenant tarama sorgularinda ABP tenant filtresini yalniz sorgu omru boyunca kapatir.
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    // DbContext provider'ini Foundation repository tabanina devreder.
    public EfCoreTestScenarioRepository(
        IDbContextProvider<TestModuleDbContext> dbContextProvider,
        IDataFilter<IMultiTenant> multiTenantFilter)
        : base(dbContextProvider)
    {
        _multiTenantFilter = multiTenantFilter;
    }

    // Senaryo anahtarinin en yuksek version numarali satirini getirir.
    public async Task<TestScenario?> FindLatestVersionAsync(
        string scenarioKey,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable
            .Where(entity => entity.ScenarioKey == scenarioKey)
            .OrderByDescending(entity => entity.VersionNo)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    // Senaryo anahtarinin yayinlanmis en yeni surumunu getirir.
    public async Task<TestScenario?> FindPublishedAsync(
        string scenarioKey,
        Guid publishedStateId,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable
            .Where(entity => entity.ScenarioKey == scenarioKey && entity.StateId == publishedStateId)
            .OrderByDescending(entity => entity.VersionNo)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    // Mevcut azami version degerini SQL'de hesaplayip siradaki numarayi dondurur.
    public async Task<int> GetNextVersionNoAsync(
        string scenarioKey,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        var latestVersion = await queryable
            .Where(entity => entity.ScenarioKey == scenarioKey)
            .Select(entity => (int?)entity.VersionNo)
            .MaxAsync(GetCancellationToken(cancellationToken));
        return latestVersion.GetValueOrDefault() + 1;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TestScenario>> GetExpiredQuarantinesAsync(
        DateTime expiredBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        // Supurucu CurrentTenant olmadan calisir; ABP tenant filtresi bu sorguyu "TenantId == null"a indirger
        // ve tarama bosalir. Capraz tenant okuma yalniz bu sorgunun omru boyunca ve yalniz tenant filtresi
        // icin acilir; global filtreleri toptan kaldiran EF yolu kullanilmaz.
        using (_multiTenantFilter.Disable())
        {
            var queryable = await GetQueryableAsync();
            return await queryable
                .Where(entity => entity.QuarantineUntil != null && entity.QuarantineUntil <= expiredBefore)
                .OrderBy(entity => entity.QuarantineUntil)
                .ThenBy(entity => entity.Id)
                .Take(maxResultCount)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
    }
}
