using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Her entry'nin uygunluk kontrolunden gecmesini, veritabani adiminin yeniden cagrilmamasini ve bulgu kaynak kodlarini dogrular.
// sistemdeki gorevi: Uc hakemin kayit sahipligini ve ADR-0015 §D zamanlama kuralini koda baglar.
public class OracleDispatchManagerTests
{
    private const string TraceId = "0123456789abcdef0123456789abcdef";

    // Yesil adim da uygunluk gozlemi uretmeli; kontrol yalniz kirmizilara uygulanmamalidir.
    [Fact]
    public void Should_create_observation_for_passing_entry_too()
    {
        var entry = CreateApiEntry(statusCode: 200);

        var observation = new OracleDispatchManager().CreateObservation(entry, CreateContext());

        observation.Method.ShouldBe("POST");
        observation.Path.ShouldBe("/orders");
        observation.StatusCode.ShouldBe(200);
        observation.Correlation.ShouldNotBeNull();
        observation.Correlation!.StepKey.ShouldBe("create-order");
        observation.Correlation.TraceId.ShouldBe(TraceId);
    }

    // Gecen uygunluk hukmu bulgu uretmemeli ve adim Passed sayilmalidir.
    [Fact]
    public void Should_judge_passing_response_without_findings()
    {
        var judgement = new OracleDispatchManager().JudgeResponse(
            CreateApiEntry(statusCode: 200),
            new ConformanceResult { OutcomeCode = PtnOutcomeCodes.Passed });

        judgement.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Passed);
        judgement.SourceCheckerCode.ShouldBe(TestSourceCheckerCodes.ApiContract);
        judgement.FailureCategoryCode.ShouldBeNull();
        judgement.Findings.ShouldBeEmpty();
    }

    // Sozlesme reddi Contract kategorisiyle ve ApiContract kaynak koduyla bulguya donusmelidir.
    [Fact]
    public void Should_tag_conformance_findings_with_api_contract_source()
    {
        var result = new ConformanceResult
        {
            OutcomeCode = PtnOutcomeCodes.ResponseSchemaViolation,
            Violations =
            [
                new ConformanceViolation
                {
                    RuleCode = "schema",
                    JsonPointer = "/data/total",
                    Keyword = "type"
                }
            ]
        };

        var judgement = new OracleDispatchManager().JudgeResponse(CreateApiEntry(statusCode: 200), result);

        judgement.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Failed);
        judgement.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Contract);
        judgement.Findings.Count.ShouldBe(1);
        judgement.Findings[0].SourceCheckerCode.ShouldBe(TestSourceCheckerCodes.ApiContract);
        judgement.Findings[0].RuleRef.ShouldBe("schema");
        judgement.Findings[0].Location.ShouldBe("POST /orders#/data/total");
    }

    // Veritabani adimi HAR yanitindan okunmali, checker'a ikinci kez sorulmamalidir (ADR-0015 §D).
    [Fact]
    public void Should_read_database_assertion_from_har_response()
    {
        var entry = CreateDatabaseEntry("""{"outcomeCode":"RowNotFound"}""");

        var judgement = new OracleDispatchManager().JudgeDatabaseAssertion(entry);

        judgement.SourceCheckerCode.ShouldBe(TestSourceCheckerCodes.DatabaseComparison);
        judgement.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Failed);
        judgement.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Persistence);
        judgement.CheckerOutcomeCode.ShouldBe(PtnOutcomeCodes.RowNotFound);
        judgement.Findings[0].SourceCheckerCode.ShouldBe(TestSourceCheckerCodes.DatabaseComparison);
    }

    // Okunamayan assertion yaniti kalicilik hukmu vermemeli, belirsiz kalmalidir.
    [Fact]
    public void Should_mark_unreadable_assertion_response_inconclusive()
    {
        var judgement = new OracleDispatchManager().JudgeDatabaseAssertion(CreateDatabaseEntry("<html/>"));

        judgement.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Inconclusive);
        judgement.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Technical);
        judgement.ErrorCode.ShouldBe(TestModuleRunErrorCodes.AssertionResponseUnreadable);
    }

    // Adim kimligi cozulemeyen entry konuma gore eslenmemeli, Inconclusive gerekce tasimalidir.
    [Fact]
    public void Should_mark_unbound_entry_inconclusive()
    {
        var entry = CreateApiEntry(statusCode: 500);
        entry.StepKey = null;

        var judgement = new OracleDispatchManager().JudgeResponse(
            entry,
            new ConformanceResult { OutcomeCode = PtnOutcomeCodes.ServerError });

        judgement.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Inconclusive);
        judgement.ErrorCode.ShouldBe(TestModuleRunErrorCodes.StepKeyMissing);
    }

    // Runner hukmu ayri kaynak koduyla tasinmali ve teshis hedefi olarak secilmemelidir.
    [Fact]
    public void Should_separate_runner_findings_from_checker_findings()
    {
        var manager = new OracleDispatchManager();
        var runner = manager.JudgeRunner(new WorkflowRunOutcome { ExitCode = 1, RunnerRef = "respect@cli" });
        var contract = manager.JudgeResponse(
            CreateApiEntry(statusCode: 500),
            new ConformanceResult { OutcomeCode = PtnOutcomeCodes.ServerError });

        var dispatch = manager.Combine([contract, runner], diagnosis: null);

        runner.SourceCheckerCode.ShouldBe(TestSourceCheckerCodes.Runner);
        OracleDispatchManager.SelectDiagnosisTarget([runner, contract]).ShouldBe(contract);
        dispatch.Findings.Select(finding => finding.SourceCheckerCode)
            .ShouldBe([TestSourceCheckerCodes.Runner, TestSourceCheckerCodes.ApiContract]);
    }

    // Butceyi asan teshis raporu satir ici sinira indirgenmelidir.
    [Fact]
    public void Should_bound_oversized_diagnosis_report()
    {
        var diagnosis = new DiagnosisReport
        {
            SourceCheckerCode = TestSourceCheckerCodes.ApiContract,
            Title = "Schema violation",
            Detail = new string('d', 6_000)
        };

        var dispatch = new OracleDispatchManager().Combine([], diagnosis);

        dispatch.DiagnosisReport.ShouldNotBeNull();
        dispatch.DiagnosisReport!.ShouldContain("Schema violation");
    }

    // Adim kimligi echo edilmis bir API entry'si kurar.
    private static HarEntryModel CreateApiEntry(int statusCode)
    {
        return new HarEntryModel
        {
            StepKey = "create-order",
            Ordinal = 2,
            Method = "POST",
            Url = "https://api.test/orders",
            StatusCode = statusCode,
            ResponseContentType = "application/json",
            ResponseBody = """{"data":{"total":1}}""",
            StartedAtMs = 10
        };
    }

    // Derlenmis veritabani assertion adimina karsilik gelen entry kurar.
    private static HarEntryModel CreateDatabaseEntry(string responseBody)
    {
        return new HarEntryModel
        {
            StepKey = "verify-subject-row",
            Ordinal = 1,
            Method = "POST",
            Url = "https://checker.test/api/comparison/assertions/row",
            StatusCode = 200,
            ResponseContentType = "application/json",
            ResponseBody = responseBody,
            IsDatabaseAssertion = true
        };
    }

    // Ortam baglamasini tasiyan icra baglamini kurar.
    private static TestRunExecutionContext CreateContext()
    {
        return new TestRunExecutionContext
        {
            TraceId = TraceId,
            EnvironmentBinding = new TestRunEnvironmentBinding
            {
                EnvironmentKey = "staging",
                BaseUrl = "https://api.test"
            },
            Inputs = new Dictionary<string, string>()
        };
    }
}
