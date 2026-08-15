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

    /// <inheritdoc />
    public async Task<IReadOnlyList<TestRun>> GetExpiredHarArtifactsAsync(
        DateTime completedBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable
            .AsNoTracking()
            .Where(entity => entity.CompletedAt.HasValue &&
                             entity.CompletedAt.Value < completedBefore &&
                             entity.HarBlobName != null)
            .OrderBy(entity => entity.CompletedAt)
            .Take(maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TestRun>> GetExpiredRunsAsync(
        DateTime completedBefore,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable
            .AsNoTracking()
            .Where(entity => entity.CompletedAt.HasValue && entity.CompletedAt.Value < completedBefore)
            .OrderBy(entity => entity.CompletedAt)
            .Take(maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    /// <inheritdoc />
    public async Task ClearHarArtifactNamesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var queryable = await GetQueryableAsync();
        await queryable
            .Where(entity => ids.Contains(entity.Id))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(entity => entity.HarBlobName, (string?)null),
                GetCancellationToken(cancellationToken));
    }

    /// <inheritdoc />
    public async Task DeleteExpiredRunsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var queryable = await GetQueryableAsync();
        await queryable
            .Where(entity => ids.Contains(entity.Id))
            .ExecuteDeleteAsync(GetCancellationToken(cancellationToken));
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

        var result = await dbContext.Set<TestRunResult>()
            .Include(entity => entity.Findings)
            .Where(entity => entity.TestRunId == id)
            .OrderByDescending(entity => entity.Attempt)
            .FirstOrDefaultAsync(token);

        return new TestRunReport
        {
            Run = run,
            Result = result,
            OutcomeCode = await ReadOutcomeCodeAsync(dbContext, result, token),
            PreviousOutcomeCode = await ReadPreviousOutcomeCodeAsync(dbContext, run, token)
        };
    }

    // Terminal denemenin lookup kimligini kararli hukum koduna cevirir.
    /// <summary>Verilen sonucun terminal hukum kodunu getirir.</summary>
    private static async Task<string?> ReadOutcomeCodeAsync(
        TestModuleDbContext dbContext,
        TestRunResult? result,
        CancellationToken cancellationToken)
    {
        if (result is null)
        {
            return null;
        }

        return await dbContext.Set<TestOutcomeStatus>()
            .Where(outcome => outcome.Id == result.OutcomeStatusId)
            .Select(outcome => outcome.Code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Ayni trend kovasindaki bir onceki kosumun terminal hukmunu tek sorguda getirir.
    /// <summary>Ayni history kovasindaki onceki kosumun terminal hukum kodunu getirir.</summary>
    private static async Task<string?> ReadPreviousOutcomeCodeAsync(
        TestModuleDbContext dbContext,
        TestRun run,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(run.HistoryId))
        {
            return null;
        }

        return await dbContext.Set<TestRun>()
            .Where(previous => previous.HistoryId == run.HistoryId &&
                               previous.Id != run.Id &&
                               previous.CreationTime < run.CreationTime)
            .OrderByDescending(previous => previous.CreationTime)
            .Join(
                dbContext.Set<TestRunResult>(),
                previous => previous.Id,
                previousResult => previousResult.TestRunId,
                (previous, previousResult) => previousResult)
            .OrderByDescending(previousResult => previousResult.Attempt)
            .Join(
                dbContext.Set<TestOutcomeStatus>(),
                previousResult => previousResult.OutcomeStatusId,
                outcome => outcome.Id,
                (previousResult, outcome) => outcome.Code)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
