using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Kosum kabul kapisini, severity haritasini ve runner cagri planinin guvenlik kurallarini dogrular.
// sistemdeki gorevi: SCHEMA_CHECK'in kayit sahipliginin Respect'e devredilmesini ve secret'in argument'e dusmesini engeller.
public class WorkflowRunPlannerTests
{
    private const string SecretValue = "s3cr3t-token";
    private const string TraceId = "0123456789abcdef0123456789abcdef";

    // Dort kontrolun tamami her kosumda acikca set edilmeli; SCHEMA_CHECK kalici hukmu vermemeli.
    [Fact]
    public async Task Should_set_every_respect_check_severity_explicitly()
    {
        var request = await CreatePlanner().CreateRequestAsync(CreateContext(CreateFacts()));

        request.SeverityMap.Count.ShouldBe(4);
        request.SeverityMap[RespectCheckCodes.StatusCodeCheck].ShouldBe(RespectSeverityCodes.Error);
        request.SeverityMap[RespectCheckCodes.SuccessCriteriaCheck].ShouldBe(RespectSeverityCodes.Error);
        request.SeverityMap[RespectCheckCodes.SchemaCheck].ShouldBe(RespectSeverityCodes.Warn);
        request.SeverityMap[RespectCheckCodes.ContentTypeCheck].ShouldBe(RespectSeverityCodes.Warn);
    }

    // Yalniz 1.0.1 kosulabilir; 1.1 belgesi surec baslamadan reddedilmeli (AUDIT-0002 BULGU-07).
    [Fact]
    public async Task Should_reject_unsupported_arazzo_version()
    {
        var facts = CreateFacts();
        facts.ArazzoVersion = "1.1";

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            CreatePlanner().CreateRequestAsync(CreateContext(facts)));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.ArazzoVersionUnsupported);
    }

    // Runner XPath criterion'u desteklemez; belge kosuma hic girmemeli.
    [Fact]
    public async Task Should_reject_xpath_criteria()
    {
        var facts = CreateFacts();
        facts.HasXPathCriterion = true;

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            CreatePlanner().CreateRequestAsync(CreateContext(facts)));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.XPathCriteriaRejected);
    }

    // Girdiler ortam degiskeniyle gecmeli; secret process listesine dusmemeli (AUDIT-0002 BULGU-09).
    [Fact]
    public async Task Should_pass_inputs_through_environment_variable_only()
    {
        var planner = CreatePlanner();
        var request = await planner.CreateRequestAsync(CreateContext(CreateFacts()));

        var plan = await planner.CreatePlanAsync(request, "/tmp/run");

        plan.EnvironmentVariables[WorkflowRunnerConsts.InputEnvironmentVariableName].ShouldContain(SecretValue);
        plan.Arguments.Any(argument => argument.Contains(SecretValue, StringComparison.Ordinal)).ShouldBeFalse();
        plan.Arguments.ShouldContain(WorkflowRunnerConsts.InputEnvironmentVariableName);
    }

    // Maskeleme hicbir kod yolunda kapatilmamali (ADR-0015 §G).
    [Fact]
    public async Task Should_never_disable_secret_masking()
    {
        var planner = CreatePlanner();
        var request = await planner.CreateRequestAsync(CreateContext(CreateFacts()));

        var plan = await planner.CreatePlanAsync(request, "/tmp/run");

        plan.Arguments
            .Any(argument => argument.Contains("no-secrets-masking", StringComparison.OrdinalIgnoreCase))
            .ShouldBeFalse();
        plan.Arguments.ShouldContain($"--execution-timeout={WorkflowRunnerConsts.DefaultExecutionTimeoutSeconds}");
        plan.Arguments.ShouldContain($"--max-fetch-timeout={WorkflowRunnerConsts.DefaultMaxFetchTimeoutSeconds}");
    }

    // Sert kill butcesi runner butcesinin uzerine ek sure taniyarak asili surec birakmamali.
    [Fact]
    public async Task Should_budget_hard_kill_above_runner_timeout()
    {
        var planner = CreatePlanner();
        var request = await planner.CreateRequestAsync(CreateContext(CreateFacts()));

        var plan = await planner.CreatePlanAsync(request, "/tmp/run");

        plan.HardKillMs.ShouldBe(
            (WorkflowRunnerConsts.DefaultExecutionTimeoutSeconds * 1_000) + WorkflowRunnerConsts.HardKillGraceMs);
        plan.RunnerRef.ShouldContain(WorkflowRunnerConsts.RedoclyCliImage);
    }

    // Pinli imaj ve butce ayarlarini varsayilana dusuren setting provider ile planner kurar.
    private static WorkflowRunPlanner CreatePlanner()
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return new WorkflowRunPlanner(settingProvider);
    }

    // Kabul kapisini gecen varsayilan belge olgularini kurar.
    private static WorkflowDocumentFacts CreateFacts()
    {
        return new WorkflowDocumentFacts
        {
            ArazzoVersion = WorkflowRunnerConsts.ArazzoTargetVersion,
            HasXPathCriterion = false,
            StepKeys = ["create-order"]
        };
    }

    // Secret tasiyan girdilerle icra baglamini kurar.
    private static TestRunExecutionContext CreateContext(WorkflowDocumentFacts facts)
    {
        return new TestRunExecutionContext
        {
            TestRunId = Guid.NewGuid(),
            CompiledDocument = "arazzo: 1.0.1",
            DocumentFacts = facts,
            Inputs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [WorkflowRunnerConsts.Inputs.BaseUrl] = "https://api.test",
                ["apiToken"] = SecretValue
            },
            TraceId = TraceId
        };
    }
}
