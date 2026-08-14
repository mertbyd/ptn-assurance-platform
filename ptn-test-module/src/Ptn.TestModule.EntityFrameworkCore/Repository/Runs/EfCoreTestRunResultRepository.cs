using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexum.Abp.Foundation.EntityFrameworkCore.Repositories;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Interface.Runs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore.Repository.Runs;

// islevi: Terminal deneme ve bulgulu aggregate sorgularinin EF Core uygulamasini saglar.
// sistemdeki gorevi: Attempt uretimi ve rapor Include sorgusunu provider katmaninda tutar.
/// <summary>
/// TestRunResult repository sozlesmesinin EF Core uygulamasidir.
/// </summary>
[ExposeServices(typeof(ITestRunResultRepository))]
public class EfCoreTestRunResultRepository
    : BaseEfCoreRepository<TestModuleDbContext, TestRunResult, Guid>, ITestRunResultRepository
{
    // DbContext provider'ini Foundation repository tabanina devreder.
    /// <summary>Repository'yi TestModuleDbContext provider ile kurar.</summary>
    public EfCoreTestRunResultRepository(IDbContextProvider<TestModuleDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    // Belirli attempt'i veya null istekte en son attempt'i SQL'de secer.
    /// <summary>Bir kosumun istenen veya en son terminal denemesini getirir.</summary>
    public async Task<TestRunResult?> FindByAttemptAsync(
        Guid testRunId,
        int? attempt = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        var runResults = queryable.Where(entity => entity.TestRunId == testRunId);
        if (attempt.HasValue)
        {
            return await runResults.FirstOrDefaultAsync(
                entity => entity.Attempt == attempt.Value,
                GetCancellationToken(cancellationToken));
        }

        return await runResults
            .OrderByDescending(entity => entity.Attempt)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    // Aggregate'i bulgu cocuklariyla tek Include sorgusunda getirir.
    /// <summary>Terminal sonucu tum findings cocuklariyla tek sorguda getirir.</summary>
    public async Task<TestRunResult?> GetWithFindingsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable
            .Include(entity => entity.Findings)
            .FirstOrDefaultAsync(
                entity => entity.Id == id,
                GetCancellationToken(cancellationToken));
    }
}
