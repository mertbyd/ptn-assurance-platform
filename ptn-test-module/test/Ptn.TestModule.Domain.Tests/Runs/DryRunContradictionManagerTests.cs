using System;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Kirmizi kuru kosumun yonlendirmesiz celiski bildirimi urettigini dogrular.
// sistemdeki gorevi: Hukum degisikligi veya ajan tavsiyesinin TM-18 yuzeyine sizmasini engeller.
public class DryRunContradictionManagerTests
{
    [Fact]
    public void Should_report_a_red_dry_run_as_a_contradiction()
    {
        var model = new TestRunCreateModel { TestKey = "ticket", IsDryRun = true };
        var run = new TestRun(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, model,
            new TestRunEnvironmentBinding { EnvironmentKey = "test" }, "history", "0123456789abcdef0123456789abcdef", null, null, null);
        var result = new DryRunContradictionManager().Create(new TestRunReport { Run = run, OutcomeCode = TestOutcomeStatusCodes.Failed });
        result.HasContradiction.ShouldBeTrue();
        result.OutcomeCode.ShouldBe(TestOutcomeStatusCodes.Failed);
        result.Contract.ShouldNotContain("should", Case.Insensitive);
    }
}
