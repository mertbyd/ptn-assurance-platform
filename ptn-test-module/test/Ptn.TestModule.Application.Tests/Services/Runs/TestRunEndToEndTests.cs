using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Ptn.TestModule.Services.Bridge;
using Ptn.TestModule.Services.Runs;
using Shouldly;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Runs;

// islevi: Elle yazilmis bir Arazzo senaryosunun HAR'indan terminal hukme kadar olan yargi yolunu uctan uca dogrular.
// sistemdeki gorevi: Her entry'nin uygunluktan gecmesini, veritabani adiminin yeniden cagrilmamasini ve model cagrisi olmamasini korur.
public class TestRunEndToEndTests
{
    private const string TraceId = "0123456789abcdef0123456789abcdef";

    // Yesil adim da uygunluga gitmeli, veritabani adimi checker'a ikinci kez sorulmamalidir.
    [Fact]
    public async Task Should_judge_every_entry_and_never_reinvoke_database_step()
    {
        var apiOracle = CreateApiOracle();
        var judgement = await CreateService(apiOracle, CreateDiagnosis()).JudgeAsync(
            CreateContext(),
            CreateOutcome(),
            "tenant/run/trace.har");

        var observations = apiOracle.ReceivedCalls()
            .Select(call => call.GetArguments()[0])
            .OfType<ResponseObservationDto>()
            .ToList();

        observations.Count.ShouldBe(2);
        observations.Any(observation => observation.Path.Contains("assertions")).ShouldBeFalse();
        observations.Select(observation => observation.Correlation!.StepKey)
            .ShouldBe(["create-order", "read-order"], ignoreOrder: true);
        judgement.HarBlobName.ShouldBe("tenant/run/trace.har");
    }

    // Uc hakemin bulgulari kaynak koduyla ayrismali ve kirmizi adim teshise gitmelidir.
    [Fact]
    public async Task Should_separate_findings_by_source_checker_and_attach_diagnosis()
    {
        var diagnosis = CreateDiagnosis();

        var judgement = await CreateService(CreateApiOracle(), diagnosis).JudgeAsync(
            CreateContext(),
            CreateOutcome(),
            harBlobName: null);

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Failed);
        judgement.Terminal.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Persistence);
        judgement.Terminal.FailedStepName.ShouldBe("verify-subject-row");
        judgement.Terminal.DiagnosisReport.ShouldNotBeNull();
        judgement.Terminal.Findings
            .Select(finding => finding.SourceCheckerCode)
            .Distinct()
            .ShouldBe(
                [
                    TestSourceCheckerCodes.Runner,
                    TestSourceCheckerCodes.DatabaseComparison,
                    TestSourceCheckerCodes.ApiContract
                ],
                ignoreOrder: true);
        await diagnosis.Received(1).DiagnoseDatabaseAsync(Arg.Any<DiagnosisRequestDto>(), Arg.Any<CancellationToken>());
    }

    // Yargi yolu yalniz iki Bridge yuzeyini kullanmali; ajan veya model yuzeyi cagrilmamalidir (RULE-0005).
    [Fact]
    public void Should_depend_only_on_deterministic_bridge_surfaces()
    {
        var dependencies = typeof(OracleDispatchService)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToList();

        dependencies.ShouldContain(typeof(IApiOracleAppService));
        dependencies.ShouldContain(typeof(IFailureDiagnosisAppService));
        dependencies.ShouldNotContain(typeof(IPtnBridgeAppService));
        dependencies.Any(type => type.Name.Contains("Agent", StringComparison.Ordinal)).ShouldBeFalse();
    }

    // Yargi asamasinin tum Manager sahiplerini stub Bridge yuzeylerine baglar.
    private static OracleDispatchService CreateService(
        IApiOracleAppService apiOracle,
        IFailureDiagnosisAppService diagnosis)
    {
        return new OracleDispatchService(
            new HarInterpreter(),
            new OracleDispatchManager(),
            new RunOutcomeResolver(),
            apiOracle,
            diagnosis);
    }

    // Ilk API adimini gecirip ikincisini sozlesme ihlaline dusuren uygunluk yuzeyi kurar.
    private static IApiOracleAppService CreateApiOracle()
    {
        var apiOracle = Substitute.For<IApiOracleAppService>();
        apiOracle.AssertResponseAsync(Arg.Any<ResponseObservationDto>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var observation = call.Arg<ResponseObservationDto>();
                return Task.FromResult(new ConformanceResultDto
                {
                    OutcomeCode = observation.StatusCode == 201
                        ? PtnOutcomeCodes.Passed
                        : PtnOutcomeCodes.ResponseSchemaViolation,
                    Correlation = observation.Correlation
                });
            });
        return apiOracle;
    }

    // Kirmizi adim icin kararli bir teshis raporu donen yuzey kurar.
    private static IFailureDiagnosisAppService CreateDiagnosis()
    {
        var diagnosis = Substitute.For<IFailureDiagnosisAppService>();
        var report = new DiagnosisReportDto
        {
            SourceCheckerCode = TestSourceCheckerCodes.DatabaseComparison,
            Title = "Row not found",
            Status = 422,
            Detail = "Expected row was absent"
        };
        diagnosis.DiagnoseDatabaseAsync(Arg.Any<DiagnosisRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(report));
        diagnosis.DiagnoseApiAsync(Arg.Any<DiagnosisRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(report));
        return diagnosis;
    }

    // Elle yazilmis senaryonun adim kimliklerini tasiyan icra baglamini kurar.
    private static TestRunExecutionContext CreateContext()
    {
        return new TestRunExecutionContext
        {
            TestRunId = Guid.NewGuid(),
            CompiledDocument = "arazzo: 1.0.1",
            DocumentFacts = new WorkflowDocumentFacts
            {
                ArazzoVersion = "1.0.1",
                StepKeys = ["create-order", "verify-subject-row", "read-order"]
            },
            Inputs = new Dictionary<string, string>(),
            EnvironmentBinding = new TestRunEnvironmentBinding
            {
                EnvironmentKey = "staging",
                BaseUrl = "https://api.test",
                SpecSnapshotId = Guid.NewGuid(),
                DbConnectionId = Guid.NewGuid()
            },
            TraceId = TraceId
        };
    }

    // Kontrol basarisizligini bildiren runner ciktisini HAR ile birlikte kurar.
    private static WorkflowRunOutcome CreateOutcome()
    {
        return new WorkflowRunOutcome
        {
            ExitCode = 1,
            HarContent = HarContent,
            JsonSummary = """{"checks":4}""",
            DurationMs = 1_234,
            RunnerRef = "respect@redocly/cli:2.14.0"
        };
    }

    private const string HarContent = """
        {
          "log": {
            "version": "1.2",
            "creator": { "name": "respect", "version": "2.14.0" },
            "entries": [
              {
                "startedDateTime": "2026-08-14T10:00:00.000Z",
                "time": 30,
                "request": { "method": "POST", "url": "https://api.test/orders" },
                "response": {
                  "status": 201,
                  "content": {
                    "mimeType": "application/json",
                    "text": "{\"data\":{\"id\":1},\"correlation\":{\"stepKey\":\"create-order\"}}"
                  }
                }
              },
              {
                "startedDateTime": "2026-08-14T10:00:01.000Z",
                "time": 15,
                "request": { "method": "POST", "url": "https://checker.test/api/comparison/assertions/row" },
                "response": {
                  "status": 200,
                  "content": {
                    "mimeType": "application/json",
                    "text": "{\"outcomeCode\":\"RowNotFound\",\"correlation\":{\"stepKey\":\"verify-subject-row\"}}"
                  }
                }
              },
              {
                "startedDateTime": "2026-08-14T10:00:02.000Z",
                "time": 9,
                "request": { "method": "GET", "url": "https://api.test/orders/1" },
                "response": {
                  "status": 200,
                  "content": {
                    "mimeType": "application/json",
                    "text": "{\"data\":{\"total\":\"1\"},\"correlation\":{\"stepKey\":\"read-order\"}}"
                  }
                }
              }
            ]
          }
        }
        """;
}
