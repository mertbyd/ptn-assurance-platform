using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace Ptn.TestModule.EntityFrameworkCore.Repository.Runs;

// islevi: Senaryo saglik materialized view'inin sorgu ve yenileme SQL'ini kalicilik katmaninda tutar.
// sistemdeki gorevi: Anahtarsiz view ABP entity'si olmadigi icin tenant kapisi bu tek okuma yolunda acikca uygulanir.
[ExposeServices(typeof(IScenarioHealthRepository))]
public class EfCoreScenarioHealthRepository : IScenarioHealthRepository, ITransientDependency
{
    // Okuyuculari bloklamayan yenileme komutunun degismez govdesidir.
    private const string RefreshStatementPrefix = "REFRESH MATERIALIZED VIEW CONCURRENTLY ";

    // View satirlari ABP IMultiTenant filtresine girmez; tenant anahtari her sorguda acikca eslenir.
    private readonly ICurrentTenant _currentTenant;
    private readonly IDbContextProvider<TestModuleDbContext> _dbContextProvider;

    public EfCoreScenarioHealthRepository(
        IDbContextProvider<TestModuleDbContext> dbContextProvider,
        ICurrentTenant currentTenant)
    {
        _dbContextProvider = dbContextProvider;
        _currentTenant = currentTenant;
    }

    /// <inheritdoc />
    public async Task<ScenarioHealthPage> GetPageAsync(
        ScenarioHealthQuery query,
        CancellationToken cancellationToken = default)
    {
        var filtered = (await CreateTenantQueryAsync())
            .Where(row => (query.ScenarioKey == null || row.ScenarioKey == query.ScenarioKey) &&
                          (query.MinFlakyRatio == null || row.FlakyRatio >= query.MinFlakyRatio) &&
                          (query.MaxPassRatio == null || row.PassRatio <= query.MaxPassRatio));

        var totalCount = await filtered.LongCountAsync(cancellationToken);
        var ordered = query.Sorting switch
        {
            ScenarioHealthQueryFields.FlakyRatio => filtered.OrderByDescending(row => row.FlakyRatio),
            ScenarioHealthQueryFields.PassRatio => filtered.OrderBy(row => row.PassRatio),
            ScenarioHealthQueryFields.P95DurationMs => filtered.OrderByDescending(row => row.P95DurationMs),
            ScenarioHealthQueryFields.TotalRunCount => filtered.OrderByDescending(row => row.TotalRunCount),
            _ => filtered.OrderBy(row => row.ScenarioKey)
        };

        return new ScenarioHealthPage
        {
            TotalCount = totalCount,
            Items = await ordered
                .ThenBy(row => row.ScenarioKey)
                .Skip(query.SkipCount)
                .Take(query.MaxResultCount)
                .ToListAsync(cancellationToken)
        };
    }

    /// <inheritdoc />
    public async Task<ScenarioHealth?> FindByScenarioKeyAsync(
        string scenarioKey,
        CancellationToken cancellationToken = default)
    {
        var queryable = await CreateTenantQueryAsync();
        return await queryable.FirstOrDefaultAsync(row => row.ScenarioKey == scenarioKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        // CONCURRENTLY transaction icinde kosamaz. Cagiran UOW islemsiz acilir ve ExecuteSqlRaw kendi
        // transaction'ini kurmaz; komut baglantida dogrudan autocommit olur.
        // Sema ve view adi Domain.Shared sabitleridir; SQL tanimlayicisi parametrelenemez, kullanici girdisi girmez.
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var statement = string.Concat(
            RefreshStatementPrefix,
            TestModuleDbProperties.RunSchema,
            ".",
            ScenarioHealthConsts.ViewName,
            ";");
        await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
    }

    // Okuma yuzeyini aktif tenant'in satirlariyla sinirlar; host baglami bos Guid anahtarina karsilik gelir.
    private async Task<IQueryable<ScenarioHealth>> CreateTenantQueryAsync()
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var tenantKey = _currentTenant.Id ?? Guid.Empty;
        return dbContext.Set<ScenarioHealth>()
            .AsNoTracking()
            .Where(row => row.TenantKey == tenantKey);
    }
}
