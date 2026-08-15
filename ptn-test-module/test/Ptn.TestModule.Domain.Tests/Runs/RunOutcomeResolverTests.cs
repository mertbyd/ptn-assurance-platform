using System;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Adim hukumlerinin, malzeme kaymasinin, iptalin ve beklenmeyen hatanin terminal karsiliklarini dogrular.
// sistemdeki gorevi: Iptali Technical saymayan ve kaymayi Failed saymayan siniflandirmayi korur (ADR-0015 §E, ADR-0020 §C).
public class RunOutcomeResolverTests
{
    // Tek kirmizi adim kosu hukmunu Failed yapmali ve birincil adimi rapora tasimalidir.
    [Fact]
    public void Should_resolve_failed_run_from_step_judgements()
    {
        var dispatch = new OracleDispatchManager().Combine(
            [CreatePassed(ordinal: 1), CreateFailed(ordinal: 2)],
            diagnosis: null);

        var judgement = new RunOutcomeResolver().Resolve(dispatch, "tenant/run/trace.har");

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Failed);
        judgement.Terminal.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Contract);
        judgement.Terminal.FailedStepOrdinal.ShouldBe(2);
        judgement.Terminal.FailedStepName.ShouldBe("step-2");
        judgement.Terminal.LastCompletedOrdinal.ShouldBe(1);
        judgement.Terminal.RunStatusCode.ShouldBe(TestRunStatusCodes.Completed);
        judgement.HarBlobName.ShouldBe("tenant/run/trace.har");
    }

    // Tum adimlar gecerse Passed hukmu hicbir sorun alani tasimamalidir.
    [Fact]
    public void Should_resolve_passed_run_without_failure_data()
    {
        var dispatch = new OracleDispatchManager().Combine([CreatePassed(ordinal: 1)], diagnosis: null);

        var judgement = new RunOutcomeResolver().Resolve(dispatch, harBlobName: null);

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Passed);
        Should.NotThrow(() => TestRunResultManager.EnsurePassedInvariant(judgement.Terminal));
    }

    // Malzeme kaymasi Failed degil Inconclusive ve Technical olmalidir (ADR-0020 §C).
    [Fact]
    public void Should_resolve_material_drift_as_inconclusive_technical()
    {
        var judgement = new RunOutcomeResolver().ResolveMaterialDrift(new TestRunMaterialDrift
        {
            HasDrift = true,
            DriftedMaterialCodes = ["DbSchema"]
        });

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Inconclusive);
        judgement.Terminal.OutcomeCode.ShouldNotBe(TestOutcomeStatusCodes.Failed);
        judgement.Terminal.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Technical);
        judgement.Terminal.ErrorCode.ShouldBe(TestModuleRunErrorCodes.MaterialDriftDetected);
        judgement.Terminal.Detail.ShouldContain("DbSchema");
    }

    // Iptal Technical sayilmamali; motor durumu Cancelled olmalidir.
    [Fact]
    public void Should_resolve_cancellation_as_cancelled_run()
    {
        var judgement = new RunOutcomeResolver().ResolveFailure(
            new AggregateException(new TaskCanceledException()));

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Inconclusive);
        judgement.Terminal.FailureCategoryCode.ShouldBeNull();
        judgement.Terminal.ErrorCode.ShouldBe(TestModuleRunErrorCodes.RunCancelled);
        judgement.Terminal.RunStatusCode.ShouldBe(TestRunStatusCodes.Cancelled);
    }

    // Runner cokusu Broken olmali, bilinen domain kodu korunmali, motor Aborted sayilmalidir.
    [Fact]
    public void Should_resolve_runner_crash_as_broken()
    {
        var judgement = new RunOutcomeResolver().ResolveFailure(
            new AggregateException(new BusinessException(TestModuleRunErrorCodes.RunnerExitedNonZero)));

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Broken);
        judgement.Terminal.FailureCategoryCode.ShouldBe(TestFailureCategoryCodes.Technical);
        judgement.Terminal.ErrorCode.ShouldBe(TestModuleRunErrorCodes.RunnerExitedNonZero);
        judgement.Terminal.RunStatusCode.ShouldBe(TestRunStatusCodes.Aborted);
    }

    // Runner zaman asimi motor durumunu TimedOut yapmali ve hukmu Broken birakmalidir.
    [Fact]
    public void Should_resolve_runner_timeout_as_timed_out_run()
    {
        var judgement = new RunOutcomeResolver().ResolveFailure(
            new BusinessException(TestModuleRunErrorCodes.RunnerTimedOut));

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Broken);
        judgement.Terminal.RunStatusCode.ShouldBe(TestRunStatusCodes.TimedOut);
    }

    // Bilinmeyen hata ham ayrinti sizdirmadan kararli koda indirgenmelidir (ADR-0016 §I).
    [Fact]
    public void Should_reduce_unknown_exception_to_stable_code()
    {
        var judgement = new RunOutcomeResolver().ResolveFailure(
            new InvalidOperationException("connection string leaked"));

        judgement.Terminal.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Broken);
        judgement.Terminal.ErrorCode.ShouldBe(TestModuleRunErrorCodes.RunFailedUnexpectedly);
        judgement.Terminal.Detail.ShouldBeNull();
    }

    // Gecen bir API adim hukmu kurar.
    private static StepJudgement CreatePassed(int ordinal)
    {
        return new StepJudgement
        {
            Entry = new HarEntryModel { Ordinal = ordinal, StepKey = $"step-{ordinal}" },
            SourceCheckerCode = TestSourceCheckerCodes.ApiContract,
            OutcomeCode = TestOutcomeStatusCodes.Passed,
            CheckerOutcomeCode = PtnOutcomeCodes.Passed
        };
    }

    // Sozlesme reddi tasiyan adim hukmu kurar.
    private static StepJudgement CreateFailed(int ordinal)
    {
        return new StepJudgement
        {
            Entry = new HarEntryModel
            {
                Ordinal = ordinal,
                StepKey = $"step-{ordinal}",
                Url = "https://api.test/orders"
            },
            SourceCheckerCode = TestSourceCheckerCodes.ApiContract,
            OutcomeCode = TestOutcomeStatusCodes.Failed,
            FailureCategoryCode = TestFailureCategoryCodes.Contract,
            CheckerOutcomeCode = PtnOutcomeCodes.ResponseSchemaViolation
        };
    }
}
