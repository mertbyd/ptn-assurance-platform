using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexum.Abp.Foundation.EntityFrameworkCore.Repositories;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore.Repository.Runs;

// islevi: Kosum trace, stale ve aktif ortam sorgularinin EF Core uygulamasini saglar.
// sistemdeki gorevi: TestRun yasam dongusunun tum LINQ ve provider ayrintisini kalicilik katmaninda tutar.
/// <summary>
/// TestRun repository sozlesmesinin EF Core uygulamasidir.
/// </summary>
[ExposeServices(typeof(ITestRunRepository))]
public class EfCoreTestRunRepository
    : BaseEfCoreRepository<TestModuleDbContext, TestRun, Guid>, ITestRunRepository
{
    // DbContext provider'ini Foundation repository tabanina devreder.
    /// <summary>Repository'yi TestModuleDbContext provider ile kurar.</summary>
    public EfCoreTestRunRepository(IDbContextProvider<TestModuleDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    // Trace kimligi sorgusunu veritabaninda tek satira indirger.
    /// <summary>Verilen W3C trace kimligine sahip kosumu getirir.</summary>
    public async Task<TestRun?> FindByTraceIdAsync(
        string traceId,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable.FirstOrDefaultAsync(
            entity => entity.TraceId == traceId,
            GetCancellationToken(cancellationToken));
    }

    // Running ve baslangic esigi filtrelerini SQL'de uygulayip toplu recovery girdisi getirir.
    /// <summary>Esik zamandan once baslamis Running kosumlari toplu olarak getirir.</summary>
    public async Task<IReadOnlyList<TestRun>> GetStaleRunningAsync(
        Guid runningStatusId,
        DateTime startedBefore,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable
            .Where(entity => entity.RunStatusId == runningStatusId &&
                             entity.StartedAt.HasValue &&
                             entity.StartedAt.Value < startedBefore)
            .OrderBy(entity => entity.StartedAt)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    // Kosumu, tum denemelerini ve bulgularini hukum kodu lookup'a join edilerek tek sorguda getirir.
    /// <summary>Kosumun deterministik ihracat girdisini getirir.</summary>
    public async Task<RunExportSource?> GetExportSourceAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var token = GetCancellationToken(cancellationToken);
        var dbContext = await GetDbContextAsync();
        var run = await dbContext.Set<TestRun>()
            .FirstOrDefaultAsync(entity => entity.Id == id, token);
        if (run is null)
        {
            return null;
        }

        return new RunExportSource
        {
            Run = run,
            Attempts = await dbContext.Set<TestRunResult>()
                .Include(entity => entity.Findings)
                .Where(entity => entity.TestRunId == id)
                .OrderBy(entity => entity.Attempt)
                .Join(
                    dbContext.Set<TestOutcomeStatus>(),
                    result => result.OutcomeStatusId,
                    outcome => outcome.Id,
                    (result, outcome) => new RunExportAttempt
                    {
                        Attempt = result.Attempt,
                        OutcomeCode = outcome.Code,
                        DurationMs = result.DurationMs,
                        ErrorCode = result.ErrorCode,
                        Detail = result.Detail,
                        FailedStepName = result.FailedStepName,
                        FailedStepOrdinal = result.FailedStepOrdinal,
                        Findings = result.Findings.OrderBy(finding => finding.Ordinal).ToList()
                    })
                .ToListAsync(token)
        };
    }

    // Kosumu ve en son denemeyi getirir; bulgular tek Include ile gelir, bulgu basina sorgu acilmaz.
    /// <summary>Kosumun bulgulu ve teshisli terminal raporunu getirir.</summary>
    public async Task<TestRunReport?> GetReportAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var token = GetCancellationToken(cancellationToken);
        var dbContext = await GetDbContextAsync();
        var run = await dbContext.Set<TestRun>()
            .FirstOrDefaultAsync(entity => entity.Id == id, token);
        if (run is null)
        {
            return null;
        }

        return new TestRunReport
        {
            Run = run,
            Result = await dbContext.Set<TestRunResult>()
                .Include(entity => entity.Findings)
                .Where(entity => entity.TestRunId == id)
                .OrderByDescending(entity => entity.Attempt)
                .FirstOrDefaultAsync(token)
        };
    }

    // Ortam ve aktif durum kumesini SQL Any sorgusuyla kontrol eder.
    /// <summary>Verilen ortamda belirtilen durumlardan birinde kosum olup olmadigini bildirir.</summary>
    public async Task<bool> ExistsActiveForEnvironmentAsync(
        string environmentKey,
        IReadOnlyCollection<Guid> activeStatusIds,
        CancellationToken cancellationToken = default)
    {
        var statusIds = activeStatusIds.ToArray();
        var queryable = await GetQueryableAsync();
        return await queryable.AnyAsync(
            entity => entity.EnvironmentKey == environmentKey && statusIds.Contains(entity.RunStatusId),
            GetCancellationToken(cancellationToken));
    }
}
