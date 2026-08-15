using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexum.Abp.Foundation.EntityFrameworkCore.Repositories;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Catalog;
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

    /// <summary>Filtrelenmis kosumlari lookup kodlari ve son deneme sayaclariyla projekte eder.</summary>
    public async Task<TestRunHeaderPage> GetHeaderPageAsync(
        TestRunQuery query,
        CancellationToken cancellationToken = default)
    {
        var token = GetCancellationToken(cancellationToken);
        var dbContext = await GetDbContextAsync();
        var projected =
            from run in dbContext.Set<TestRun>().AsNoTracking()
            join status in dbContext.Set<TestRunStatus>() on run.RunStatusId equals status.Id
            join trigger in dbContext.Set<TestTriggerKind>() on run.TriggerKindId equals trigger.Id
            let latest = dbContext.Set<TestRunResult>()
                .Where(result => result.TestRunId == run.Id)
                .OrderByDescending(result => result.Attempt)
                .FirstOrDefault()
            where (query.RunStatusCode == null || status.Code == query.RunStatusCode) &&
                  (query.EnvironmentKey == null || run.EnvironmentKey == query.EnvironmentKey) &&
                  (query.ScenarioId == null || run.ScenarioId == query.ScenarioId) &&
                  (query.TriggerKindCode == null || trigger.Code == query.TriggerKindCode) &&
                  (query.CreatedFrom == null || run.CreationTime >= query.CreatedFrom) &&
                  (query.CreatedTo == null || run.CreationTime <= query.CreatedTo) &&
                  (query.IsDryRun == null || run.IsDryRun == query.IsDryRun)
            select new TestRunHeader
            {
                Id = run.Id,
                ScenarioId = run.ScenarioId,
                TestKey = run.TestKey,
                EnvironmentKey = run.EnvironmentKey,
                RunStatusCode = status.Code,
                OutcomeCode = latest == null
                    ? null
                    : dbContext.Set<TestOutcomeStatus>()
                        .Where(outcome => outcome.Id == latest.OutcomeStatusId)
                        .Select(outcome => outcome.Code)
                        .FirstOrDefault(),
                TriggerKindCode = trigger.Code,
                DurationMs = latest == null ? null : latest.DurationMs,
                FindingCount = latest == null ? 0 : latest.Findings.Count,
                Attempt = latest == null ? null : latest.Attempt,
                StartedAt = run.StartedAt,
                CompletedAt = run.CompletedAt,
                IsDryRun = run.IsDryRun,
                CreationTime = run.CreationTime
            };

        var totalCount = await projected.LongCountAsync(token);
        var ordered = query.Sorting switch
        {
            TestRunQueryFields.StartedAt => projected.OrderBy(item => item.StartedAt),
            TestRunQueryFields.CompletedAt => projected.OrderBy(item => item.CompletedAt),
            TestRunQueryFields.DurationMs => projected.OrderBy(item => item.DurationMs),
            TestRunQueryFields.TestKey => projected.OrderBy(item => item.TestKey),
            TestRunQueryFields.EnvironmentKey => projected.OrderBy(item => item.EnvironmentKey),
            _ => projected.OrderByDescending(item => item.CreationTime)
        };
        return new TestRunHeaderPage
        {
            TotalCount = totalCount,
            Items = await ordered.Skip(query.SkipCount).Take(query.MaxResultCount).ToListAsync(token)
        };
    }

    /// <summary>Filtrelenmis bulgulari kosum ve hukum kodlariyla agir deger kolonlari olmadan projekte eder.</summary>
    public async Task<TestFindingHeaderPage> GetFindingPageAsync(
        TestFindingQuery query,
        CancellationToken cancellationToken = default)
    {
        var token = GetCancellationToken(cancellationToken);
        var dbContext = await GetDbContextAsync();
        var projected =
            from finding in dbContext.Set<TestResultFinding>().AsNoTracking()
            join result in dbContext.Set<TestRunResult>() on finding.TestRunResultId equals result.Id
            join run in dbContext.Set<TestRun>() on result.TestRunId equals run.Id
            join outcome in dbContext.Set<TestOutcomeStatus>() on result.OutcomeStatusId equals outcome.Id
            where (query.TestRunId == null || run.Id == query.TestRunId) &&
                  (query.ScenarioId == null || run.ScenarioId == query.ScenarioId) &&
                  (query.OutcomeCode == null || outcome.Code == query.OutcomeCode) &&
                  (query.SeverityCode == null ||
                   (query.SeverityCode == SarifReportConsts.Level.Error &&
                    (outcome.Code == TestOutcomeStatusCodes.Failed ||
                     outcome.Code == TestOutcomeStatusCodes.Broken ||
                     outcome.Code == TestOutcomeStatusCodes.Inconclusive))) &&
                  (query.SourceCheckerCode == null || finding.SourceCheckerCode == query.SourceCheckerCode) &&
                  (query.RuleRef == null || finding.RuleRef == query.RuleRef) &&
                  (query.Fingerprints.Count == 0 || query.Fingerprints.Contains(finding.Fingerprint)) &&
                  (query.CreatedFrom == null || finding.CreationTime >= query.CreatedFrom) &&
                  (query.CreatedTo == null || finding.CreationTime <= query.CreatedTo)
            select new TestFindingHeader
            {
                Id = finding.Id,
                TestRunId = run.Id,
                ScenarioId = run.ScenarioId,
                TestRunResultId = result.Id,
                Attempt = result.Attempt,
                OutcomeCode = outcome.Code,
                SeverityCode = outcome.Code == TestOutcomeStatusCodes.Failed ||
                               outcome.Code == TestOutcomeStatusCodes.Broken ||
                               outcome.Code == TestOutcomeStatusCodes.Inconclusive
                    ? SarifReportConsts.Level.Error
                    : null,
                Ordinal = finding.Ordinal,
                Fingerprint = finding.Fingerprint,
                SourceCheckerCode = finding.SourceCheckerCode,
                ComparisonKindCode = finding.ComparisonKindCode,
                RuleRef = finding.RuleRef,
                Location = finding.Location,
                Message = finding.Message,
                CreationTime = finding.CreationTime
            };

        var totalCount = await projected.LongCountAsync(token);
        var ordered = query.Sorting == TestRunQueryFields.Attempt
            ? projected.OrderBy(item => item.Attempt).ThenBy(item => item.Ordinal)
            : projected.OrderByDescending(item => item.CreationTime).ThenBy(item => item.Ordinal);
        return new TestFindingHeaderPage
        {
            TotalCount = totalCount,
            Items = await ordered.Skip(query.SkipCount).Take(query.MaxResultCount).ToListAsync(token)
        };
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScenarioCoverageRuleGroup>> GetRuleCoverageAsync(
        Guid publishedStateId,
        CancellationToken cancellationToken = default)
    {
        var token = GetCancellationToken(cancellationToken);
        var dbContext = await GetDbContextAsync();
        var projected =
            from finding in dbContext.Set<TestResultFinding>().AsNoTracking()
            join result in dbContext.Set<TestRunResult>() on finding.TestRunResultId equals result.Id
            join run in dbContext.Set<TestRun>() on result.TestRunId equals run.Id
            join scenario in dbContext.Set<TestScenario>() on run.ScenarioId equals scenario.Id
            where scenario.StateId == publishedStateId && finding.RuleRef != null
            select new { finding.RuleRef, scenario.ScenarioKey };

        return await projected
            .GroupBy(item => item.RuleRef!)
            .Select(group => new ScenarioCoverageRuleGroup
            {
                RuleRef = group.Key,
                ScenarioCount = group.Select(item => item.ScenarioKey).Distinct().Count(),
                FindingCount = group.Count()
            })
            .OrderBy(group => group.RuleRef)
            .ToListAsync(token);
    }

    /// <inheritdoc />
    public async Task<TestRun?> FindByTriggerAsync(
        string triggerKindCode,
        string triggerRef,
        Guid? scenarioId,
        CancellationToken cancellationToken = default)
    {
        var token = GetCancellationToken(cancellationToken);
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<TestRun>()
            .AsNoTracking()
            .Join(
                dbContext.Set<TestTriggerKind>(),
                run => run.TriggerKindId,
                trigger => trigger.Id,
                (run, trigger) => new { Run = run, Trigger = trigger })
            .Where(item => item.Trigger.Code == triggerKindCode &&
                           item.Run.TriggerRef == triggerRef &&
                           (scenarioId == null || item.Run.ScenarioId == scenarioId))
            .OrderBy(item => item.Run.CreationTime)
            .Select(item => item.Run)
            .FirstOrDefaultAsync(token);
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
