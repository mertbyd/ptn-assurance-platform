using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Data;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.TestModule.EntityFrameworkCore.Runs;

// islevi: Kosum tablolari unique, cascade, Restrict ve cocuk tenant davranislarini gercek SQLite modelinde dogrular.
// sistemdeki gorevi: DBML iliskileri ile ABP entity-tipi tenant filtresinin migration oncesi regression kapisidir.
/// <summary>
/// Test kosum aggregate'lerinin EF Core kalicilik ve tenant izolasyonu testleridir.
/// </summary>
public class TestRunPersistenceTests : TestModuleEntityFrameworkCoreTestBase
{
    // Ayni test_run_id ve attempt ciftinin ikinci yazimini veritabani constraint'iyle gurultulu reddeder.
    /// <summary>Terminal attempt unique indeksinin cift yazimi DbUpdateException ile reddettigini dogrular.</summary>
    [Fact]
    public async Task Should_reject_duplicate_run_attempt()
    {
        await RunSeedAsync();
        var tenantId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetContextAsync();
            using (CurrentTenant().Change(tenantId))
            {
                var graph = await CreateGraphAsync(context, "runs.unique", tenantId);
                context.AddRange(graph.Scenario, graph.Run, graph.Result);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();

                var duplicate = CreateResult(
                    graph.Run.Id,
                    graph.Result.OutcomeStatusId,
                    graph.Result.FailureCategoryId!.Value,
                    tenantId,
                    attempt: 1);
                context.TestRunResults.Add(duplicate);

                await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync());
                context.ChangeTracker.Clear();
            }
        });
    }

    // TestRun silinince yalniz terminal sonucu ve bulgu cocuklari cascade ile kalkmalidir.
    /// <summary>TestRun-Result-Finding cascade zincirinin veritabaninda calistigini dogrular.</summary>
    [Fact]
    public async Task Should_cascade_run_result_and_finding_deletion()
    {
        await RunSeedAsync();
        var tenantId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetContextAsync();
            using (CurrentTenant().Change(tenantId))
            {
                var graph = await CreateGraphAsync(context, "runs.cascade", tenantId);
                context.AddRange(graph.Scenario, graph.Run, graph.Result);
                await context.SaveChangesAsync();
                var resultId = graph.Result.Id;
                context.ChangeTracker.Clear();

                var run = await context.TestRuns.SingleAsync(entity => entity.Id == graph.Run.Id);
                context.TestRuns.Remove(run);
                await context.SaveChangesAsync();

                (await context.TestRunResults.CountAsync(entity => entity.Id == resultId)).ShouldBe(0);
                (await context.TestResultFindings.CountAsync(entity => entity.TestRunResultId == resultId)).ShouldBe(0);
            }
        });
    }

    // Senaryo surumu kosum tarafindan referanslanirken silme DB Restrict ile reddedilmelidir.
    /// <summary>TestScenario ile TestRun arasindaki Restrict baginin veri kaybini engelledigini dogrular.</summary>
    [Fact]
    public async Task Should_restrict_scenario_deletion_when_run_exists()
    {
        await RunSeedAsync();
        var tenantId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetContextAsync();
            using (CurrentTenant().Change(tenantId))
            {
                var graph = await CreateGraphAsync(context, "runs.restrict", tenantId);
                context.AddRange(graph.Scenario, graph.Run, graph.Result);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();

                var scenario = await context.TestScenarios.SingleAsync(entity => entity.Id == graph.Scenario.Id);
                context.TestScenarios.Remove(scenario);

                await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync());
                context.ChangeTracker.Clear();
            }
        });
    }

    // Dogrudan finding sorgusu baska tenant'in cocuk satirini ABP filtresiyle gizlemelidir.
    /// <summary>TestResultFinding entity tipinin tenant filtresini dogrudan sorguda uyguladigini dogrular.</summary>
    [Fact]
    public async Task Should_isolate_result_findings_by_tenant()
    {
        await RunSeedAsync();
        var firstTenant = Guid.NewGuid();
        var secondTenant = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetContextAsync();
            var first = await CreateGraphAsync(context, "runs.tenant-one", firstTenant);
            var second = await CreateGraphAsync(context, "runs.tenant-two", secondTenant);
            context.AddRange(first.Scenario, first.Run, first.Result, second.Scenario, second.Run, second.Result);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            using (CurrentTenant().Change(firstTenant))
            {
                var findings = await context.TestResultFindings.AsNoTracking().ToListAsync();
                findings.Count.ShouldBe(1);
                findings.Single().TenantId.ShouldBe(firstTenant);
            }
        });
    }

    // Test Module lookup seed'ini kosum grafi FK'lari icin gercek UoW'da calistirir.
    /// <summary>Global lookup kayitlarini persistence testleri icin idempotent olarak seed eder.</summary>
    private Task RunSeedAsync()
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var contributor = ServiceProvider.GetRequiredService<TestModuleLookupDataSeedContributor>();
            await contributor.SeedAsync(new DataSeedContext());
        });
    }

    // Aktif UoW'nun provider tarafindaki TestModuleDbContext ornegini getirir.
    /// <summary>Persistence testlerinin ortak EF Core context'ini cozer.</summary>
    private Task<TestModuleDbContext> GetContextAsync()
    {
        return ServiceProvider
            .GetRequiredService<IDbContextProvider<TestModuleDbContext>>()
            .GetDbContextAsync();
    }

    // ABP tenant degisim scope'unu saglayan servis ornegini cozer.
    /// <summary>Testlerde tenant filtresini degistirmek icin ICurrentTenant servisini getirir.</summary>
    private ICurrentTenant CurrentTenant()
    {
        return ServiceProvider.GetRequiredService<ICurrentTenant>();
    }

    // Senaryo, kosum, terminal sonuc ve finding'den olusan en kucuk kalici grafi kurar.
    /// <summary>Verilen tenant ve anahtar icin DBML iliskileri tam bir kosum grafi olusturur.</summary>
    private static async Task<(TestScenario Scenario, TestRun Run, TestRunResult Result)> CreateGraphAsync(
        TestModuleDbContext context,
        string testKey,
        Guid tenantId)
    {
        var scenarioStateId = await context.TestScenarioStates
            .Where(entity => entity.Code == TestScenarioStateCodes.Draft)
            .Select(entity => entity.Id)
            .SingleAsync();
        var runStatusId = await context.TestRunStatuses
            .Where(entity => entity.Code == TestRunStatusCodes.Completed)
            .Select(entity => entity.Id)
            .SingleAsync();
        var triggerKindId = await context.TestTriggerKinds
            .Where(entity => entity.Code == TestTriggerKindCodes.Manual)
            .Select(entity => entity.Id)
            .SingleAsync();
        var outcomeStatusId = await context.TestOutcomeStatuses
            .Where(entity => entity.Code == TestOutcomeStatusCodes.Inconclusive)
            .Select(entity => entity.Id)
            .SingleAsync();
        var failureCategoryId = await context.TestFailureCategories
            .Where(entity => entity.Code == TestFailureCategoryCodes.Technical)
            .Select(entity => entity.Id)
            .SingleAsync();

        var scenario = CreateScenario(scenarioStateId, testKey, tenantId);
        var run = CreateRun(scenario.Id, runStatusId, triggerKindId, testKey, tenantId);
        var result = CreateResult(run.Id, outcomeStatusId, failureCategoryId, tenantId, attempt: 1);
        return (scenario, run, result);
    }

    // Kalici graf icin muhurlu senaryo surumu veri kabugunu kurar.
    /// <summary>Verilen state, anahtar ve tenant ile TestScenario entity'si olusturur.</summary>
    private static TestScenario CreateScenario(Guid stateId, string scenarioKey, Guid tenantId)
    {
        return new TestScenario(
            Guid.NewGuid(),
            1,
            stateId,
            tenantId,
            new TestScenarioCreateModel
            {
                ScenarioKey = scenarioKey,
                Title = "Persistence scenario",
                SourceDocument = "source",
                SourceHash = Hash('a'),
                MaterialSeal = new TestScenarioMaterialSeal
                {
                    RulesFingerprint = Hash('c'),
                    SpecSnapshotId = Guid.NewGuid(),
                    SpecFingerprint = Hash('d'),
                    DbConnectionId = Guid.NewGuid(),
                    DbSchemaFingerprint = Hash('e'),
                    ProfileFingerprint = Hash('f')
                }
            });
    }

    // Kalici graf icin scenario, lookup ve ortam snapshot bagli kosum veri kabugunu kurar.
    /// <summary>Verilen FK kimlikleriyle TestRun aggregate'i olusturur.</summary>
    private static TestRun CreateRun(
        Guid scenarioId,
        Guid runStatusId,
        Guid triggerKindId,
        string testKey,
        Guid tenantId)
    {
        return new TestRun(
            Guid.NewGuid(),
            runStatusId,
            triggerKindId,
            tenantId,
            new TestRunCreateModel
            {
                ScenarioId = scenarioId,
                TestKey = testKey,
                TriggerKindCode = TestTriggerKindCodes.Manual
            },
            new TestRunEnvironmentBinding
            {
                EnvironmentKey = "staging",
                BaseUrl = "https://staging.example.test",
                SpecSnapshotId = Guid.NewGuid(),
                DbConnectionId = Guid.NewGuid(),
                SecretRef = "vault/staging"
            },
            Hash('1'),
            new string('2', 32),
            Hash('3'),
            Hash('4'),
            "redocly-respect@2.14.0");
    }

    // Kalici graf icin Inconclusive hukum ve tek finding tasiyan terminal aggregate'i kurar.
    /// <summary>Verilen run, lookup, tenant ve attempt ile TestRunResult aggregate'i olusturur.</summary>
    private static TestRunResult CreateResult(
        Guid runId,
        Guid outcomeStatusId,
        Guid failureCategoryId,
        Guid tenantId,
        int attempt)
    {
        var resultId = Guid.NewGuid();
        var finding = new TestResultFinding(
            Guid.NewGuid(),
            resultId,
            1,
            tenantId,
            new TestResultFindingModel
            {
                Ordinal = 1,
                SourceCheckerCode = TestSourceCheckerCodes.Runner,
                ComparisonKindCode = "MaterialSeal",
                RuleRef = "BR-001",
                Location = "materials.databaseSchema",
                Message = "Database schema material drifted."
            });
        var terminal = new TestRunTerminalModel
        {
            OutcomeCode = TestOutcomeStatusCodes.Inconclusive,
            FailureCategoryCode = TestFailureCategoryCodes.Technical,
            ErrorCode = "MATERIAL_DRIFT",
            Detail = "DatabaseSchema"
        };

        return new TestRunResult(
            resultId,
            runId,
            attempt,
            outcomeStatusId,
            failureCategoryId,
            25,
            tenantId,
            terminal,
            [finding]);
    }

    // Test fingerprint'lerini DBML'nin 64 karakterlik digest biciminde uretir.
    /// <summary>Verilen karakterden 64 karakterlik test fingerprint'i olusturur.</summary>
    private static string Hash(char value) => new(value, 64);
}
