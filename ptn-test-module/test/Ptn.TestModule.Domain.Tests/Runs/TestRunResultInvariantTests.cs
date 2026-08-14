using System.Collections.Generic;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Passed hukumunun sekiz sorun alanini ve diagnosis byte butcesini dogrular.
// sistemdeki gorevi: Sessiz kirli basari kaydi ile 4 KB ustu inline teshis yazimini engeller.
/// <summary>
/// TestRunResultManager terminal yazim invariant testleridir.
/// </summary>
public class TestRunResultInvariantTests
{
    // Sekiz sorun alanindan her birinin Passed hukmunde ayri ayri reddedilmesini zorunlu kilar.
    /// <summary>Passed sonucunun herhangi bir sorun alani tasimasini reddettigini dogrular.</summary>
    [Theory]
    [MemberData(nameof(GetPassedModelsCarryingFailureData))]
    public void Should_reject_each_failure_field_for_passed_outcome(TestRunTerminalModel model)
    {
        var exception = Should.Throw<BusinessException>(() =>
            TestRunResultManager.EnsurePassedInvariant(model));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.PassedOutcomeCarriesFailureData);
    }

    // Sekiz sorun alani null olan Passed modelinin kapidan gecmesini dogrular.
    /// <summary>Temiz Passed terminal modelinin kabul edildigini dogrular.</summary>
    [Fact]
    public void Should_accept_passed_outcome_without_failure_data()
    {
        Should.NotThrow(() => TestRunResultManager.EnsurePassedInvariant(
            new TestRunTerminalModel { OutcomeCode = TestOutcomeStatusCodes.Passed }));
    }

    // UTF-8 boyutu 4096 bayti asan diagnosis metnini reddeder.
    /// <summary>4 KB ustu diagnosis raporunun kararli domain koduyla reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_diagnosis_report_over_four_kilobytes()
    {
        var exception = Should.Throw<BusinessException>(() =>
            TestRunResultManager.EnsureDiagnosisReportSize(new string('x', 4097)));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.DiagnosisReportTooLarge);
    }

    // Passed invariant'indaki sekiz sorun alanini tek tek dolu modeller olarak uretir.
    /// <summary>Her seferinde yalniz bir sorun alani dolu olan Passed terminal modellerini dondurur.</summary>
    public static IEnumerable<object[]> GetPassedModelsCarryingFailureData()
    {
        yield return [Passed(model => model.FailureCategoryCode = "Technical")];
        yield return [Passed(model => model.ErrorCode = "RULE")];
        yield return [Passed(model => model.Detail = "detail")];
        yield return [Passed(model => model.FailedStepOrdinal = 1)];
        yield return [Passed(model => model.FailedStepName = "step")];
        yield return [Passed(model => model.FailedStepPath = "#/steps/0")];
        yield return [Passed(model => model.TakenBranchPath = "failure")];
        yield return [Passed(model => model.LastCompletedOrdinal = 1)];
    }

    // Tek sorun alani atamasini ortak Passed modeline uygular.
    /// <summary>Verilen alan atamasiyla bir Passed terminal modeli kurar.</summary>
    private static TestRunTerminalModel Passed(System.Action<TestRunTerminalModel> configure)
    {
        var model = new TestRunTerminalModel { OutcomeCode = TestOutcomeStatusCodes.Passed };
        configure(model);
        return model;
    }
}
