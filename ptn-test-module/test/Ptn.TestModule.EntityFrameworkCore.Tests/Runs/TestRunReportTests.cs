using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Data;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Services.Runs;
using Shouldly;
using Volo.Abp.Data;
using Xunit;

namespace Ptn.TestModule.EntityFrameworkCore.Runs;

// islevi: Rapor ucunun bulgulari ve diagnosis raporunu tasidigini, liste ucunun tasimadigini dogrular.
// sistemdeki gorevi: TM-22 okuma modelinin agir kolonlari liste sorgusuna sizdirmasini engelleyen regresyon kapisidir.
public class TestRunReportTests : TestModuleEntityFrameworkCoreTestBase
{
    // Rapor ucu terminal hukmu, bulgulari ve teshis raporunu tek okumada dondurmelidir.
    [Fact]
    public async Task Should_expose_findings_and_diagnosis_report_on_the_report_endpoint()
    {
        var runId = await CreatePersistedRunAsync("runs.report");

        var report = await WithUnitOfWorkAsync(() =>
            ServiceProvider.GetRequiredService<ITestRunAppService>().GetReportAsync(runId));

        report.Run.Id.ShouldBe(runId);
        report.Result.ShouldNotBeNull();
        report.Result!.DiagnosisReport.ShouldBe(DiagnosisReportJson);
        report.Result.Findings.Count.ShouldBe(1);
        report.Result.Findings[0].Location.ShouldBe("materials.databaseSchema");
    }

    // Liste ucu kosumu dondurmeli, fakat sozlesmesinde bulgu ve teshis alani bulunmamalidir.
    [Fact]
    public async Task Should_not_carry_findings_or_diagnosis_report_on_the_list_endpoint()
    {
        var runId = await CreatePersistedRunAsync("runs.list");

        var page = await WithUnitOfWorkAsync(() =>
            ServiceProvider.GetRequiredService<ITestRunAppService>()
                .GetListAsync(new TestRunListInput { MaxResultCount = 100 }));

        page.Items.ShouldContain(item => item.Id == runId);
        typeof(TestRunDto).GetProperty(nameof(TestRunResultDto.Findings)).ShouldBeNull();
        typeof(TestRunDto).GetProperty(nameof(TestRunResultDto.DiagnosisReport)).ShouldBeNull();
    }

    // Terminal denemesi ve tek bulgusu olan en kucuk kalici kosum grafini yazar.
    private async Task<Guid> CreatePersistedRunAsync(string testKey)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var contributor = ServiceProvider.GetRequiredService<TestModuleLookupDataSeedContributor>();
            await contributor.SeedAsync(new DataSeedContext());
        });

        return await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<Volo.Abp.EntityFrameworkCore.IDbContextProvider<TestModuleDbContext>>()
                .GetDbContextAsync();
            var runStatusId = await SingleLookupIdAsync(context.TestRunStatuses, TestRunStatusCodes.Completed);
            var triggerKindId = await SingleLookupIdAsync(context.TestTriggerKinds, TestTriggerKindCodes.Manual);
            var outcomeStatusId = await SingleLookupIdAsync(context.TestOutcomeStatuses, TestOutcomeStatusCodes.Failed);

            var run = CreateRun(runStatusId, triggerKindId, testKey);
            context.TestRuns.Add(run);
            context.TestRunResults.Add(CreateResult(run.Id, outcomeStatusId));
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            return run.Id;
        });
    }

    // Seed edilmis lookup satirinin kimligini koddan cozer.
    private static Task<Guid> SingleLookupIdAsync<TLookup>(DbSet<TLookup> set, string code)
        where TLookup : Nexum.Abp.Foundation.Lookups.LookupEntity<Guid>
    {
        return set.Where(entity => entity.Code == code).Select(entity => entity.Id).SingleAsync();
    }

    // Senaryosuz ad-hoc kosum veri kabugunu kurar.
    private static TestRun CreateRun(Guid runStatusId, Guid triggerKindId, string testKey)
    {
        return new TestRun(
            Guid.NewGuid(),
            runStatusId,
            triggerKindId,
            tenantId: null,
            new TestRunCreateModel
            {
                TestKey = testKey,
                TriggerKindCode = TestTriggerKindCodes.Manual
            },
            new TestRunEnvironmentBinding
            {
                EnvironmentKey = "staging",
                BaseUrl = "https://staging.example.test",
                SecretRef = "vault/staging"
            },
            Hash('1'),
            new string('2', 32),
            Hash('3'),
            Hash('4'),
            "redocly-respect@2.14.0");
    }

    // Teshis raporu ve tek bulgusu olan terminal aggregate'i kurar.
    private static TestRunResult CreateResult(Guid runId, Guid outcomeStatusId)
    {
        var resultId = Guid.NewGuid();
        var finding = new TestResultFinding(
            Guid.NewGuid(),
            resultId,
            1,
            tenantId: null,
            new TestResultFindingModel
            {
                Ordinal = 1,
                SourceCheckerCode = TestSourceCheckerCodes.Runner,
                ComparisonKindCode = "MaterialSeal",
                Location = "materials.databaseSchema",
                Message = "Database schema material drifted."
            });

        return new TestRunResult(
            resultId,
            runId,
            attempt: 1,
            outcomeStatusId,
            failureCategoryId: null,
            durationMs: 25,
            tenantId: null,
            new TestRunTerminalModel
            {
                OutcomeCode = TestOutcomeStatusCodes.Failed,
                DiagnosisReport = DiagnosisReportJson
            },
            [finding]);
    }

    // Satir ici 4 KB sinirinin altinda kalan ornek teshis govdesidir.
    private const string DiagnosisReportJson = """{"primary":"materials.databaseSchema"}""";

    private static string Hash(char value) => new(value, 64);
}
