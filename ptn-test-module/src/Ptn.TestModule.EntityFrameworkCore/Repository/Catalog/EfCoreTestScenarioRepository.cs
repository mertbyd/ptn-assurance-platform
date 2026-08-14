using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexum.Abp.Foundation.EntityFrameworkCore.Repositories;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore.Repository.Catalog;

// islevi: Senaryo katalog sorgularinin EF Core uygulamasini saglar.
// sistemdeki gorevi: Son surum, yayinlanmis surum ve version uretimi LINQ'ini provider katmaninda tutar.
[ExposeServices(typeof(ITestScenarioRepository))]
public class EfCoreTestScenarioRepository
    : BaseEfCoreRepository<TestModuleDbContext, TestScenario, Guid>, ITestScenarioRepository
{
    // DbContext provider'ini Foundation repository tabanina devreder.
    public EfCoreTestScenarioRepository(IDbContextProvider<TestModuleDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
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
}
