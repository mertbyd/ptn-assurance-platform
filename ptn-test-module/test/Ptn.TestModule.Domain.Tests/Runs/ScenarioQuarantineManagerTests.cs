using System;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Karantinanin sure zorunlulugunu, otomatik cikisini ve healed etiketinin ilk yesil kosum kuralini dogrular.
// sistemdeki gorevi: Suresiz karantinayi ve ikinci yesil kosumun healed sayilmasini kalici olarak engeller (PLAN-0003 TM-25/TM-28).
public class ScenarioQuarantineManagerTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    // Son kullanma tarihi verilmeyen karantina istegi reddedilmelidir.
    [Fact]
    public void Should_reject_a_quarantine_without_an_expiry()
    {
        var exception = Should.Throw<BusinessException>(
            () => new ScenarioQuarantineManager().Quarantine(CreateScenario(), until: null, reason: "flaky", Now));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.QuarantineRequiresExpiry);
    }

    // Gecmise donuk karantina bitisi reddedilmelidir.
    [Fact]
    public void Should_reject_a_quarantine_that_expires_in_the_past()
    {
        var exception = Should.Throw<BusinessException>(
            () => new ScenarioQuarantineManager().Quarantine(
                CreateScenario(),
                Now.AddMinutes(-1),
                reason: "flaky",
                Now));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.QuarantineWindowInvalid);
    }

    // Azami pencereyi asan karantina reddedilmelidir.
    [Fact]
    public void Should_reject_a_quarantine_beyond_the_maximum_window()
    {
        var exception = Should.Throw<BusinessException>(
            () => new ScenarioQuarantineManager().Quarantine(
                CreateScenario(),
                Now.AddDays(TestScenarioConsts.MaxQuarantineDays + 1),
                reason: "flaky",
                Now));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.QuarantineWindowInvalid);
    }

    // Gecerli pencere karantinayi aktif hale getirmelidir.
    [Fact]
    public void Should_activate_the_quarantine_for_a_valid_window()
    {
        var scenario = CreateScenario();

        new ScenarioQuarantineManager().Quarantine(scenario, Now.AddDays(3), "flaky", Now);

        scenario.QuarantineUntil.ShouldBe(Now.AddDays(3));
        scenario.QuarantineReason.ShouldBe("flaky");
        ScenarioQuarantineManager.IsQuarantined(scenario, Now).ShouldBeTrue();
    }

    // Suresi dolan senaryo karantinadan otomatik cikmalidir.
    [Fact]
    public void Should_release_the_scenario_when_the_window_expires()
    {
        var manager = new ScenarioQuarantineManager();
        var scenario = CreateScenario();
        manager.Quarantine(scenario, Now.AddDays(1), "flaky", Now);

        var released = manager.ReleaseExpired(scenario, Now.AddDays(2));

        released.ShouldBeTrue();
        scenario.QuarantineUntil.ShouldBeNull();
        scenario.QuarantineReason.ShouldBeNull();
        ScenarioQuarantineManager.IsQuarantined(scenario, Now.AddDays(2)).ShouldBeFalse();
    }

    // Suresi dolmamis karantina serbest birakilmamalidir.
    [Fact]
    public void Should_keep_an_active_quarantine_in_place()
    {
        var manager = new ScenarioQuarantineManager();
        var scenario = CreateScenario();
        manager.Quarantine(scenario, Now.AddDays(5), "flaky", Now);

        var released = manager.ReleaseExpired(scenario, Now.AddDays(1));

        released.ShouldBeFalse();
        scenario.QuarantineUntil.ShouldNotBeNull();
    }

    // Kirmizidan yesile donen ilk kosum healed sayilmalidir.
    [Fact]
    public void Should_mark_the_first_green_run_after_a_repair_as_healed()
    {
        TestRunResultManager.IsHealed(TestOutcomeStatusCodes.Passed, TestOutcomeStatusCodes.Failed)
            .ShouldBeTrue();
    }

    // Ust uste ikinci yesil kosum artik healed sayilmamalidir.
    [Fact]
    public void Should_not_mark_a_second_consecutive_green_run_as_healed()
    {
        TestRunResultManager.IsHealed(TestOutcomeStatusCodes.Passed, TestOutcomeStatusCodes.Passed)
            .ShouldBeFalse();
    }

    // Onceki kosumu olmayan ilk kosum healed sayilmamalidir.
    [Fact]
    public void Should_not_mark_a_first_ever_run_as_healed()
    {
        TestRunResultManager.IsHealed(TestOutcomeStatusCodes.Passed, previousOutcomeCode: null)
            .ShouldBeFalse();
    }

    // Kirmizi kosum healed sayilmamalidir.
    [Fact]
    public void Should_not_mark_a_failing_run_as_healed()
    {
        TestRunResultManager.IsHealed(TestOutcomeStatusCodes.Failed, TestOutcomeStatusCodes.Failed)
            .ShouldBeFalse();
    }

    // Healed etiketi rapor modeline yazilmalidir.
    [Fact]
    public void Should_write_the_healed_marker_onto_the_report()
    {
        var report = new TestRunReport
        {
            OutcomeCode = TestOutcomeStatusCodes.Passed,
            PreviousOutcomeCode = TestOutcomeStatusCodes.Broken
        };

        TestRunResultManager.MarkHealing(report).IsHealed.ShouldBeTrue();
    }

    // Karantina testleri icin kararli alanlari olan bir senaryo kabugu kurar.
    private static TestScenario CreateScenario()
    {
        return new TestScenario(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            versionNo: 1,
            stateId: Guid.NewGuid(),
            tenantId: null,
            new TestScenarioCreateModel
            {
                ScenarioKey = "orders.create",
                Title = "Order creation",
                SourceDocument = "arazzo: 1.0.1",
                SourceHash = new string('a', 64),
                MaterialSeal = new TestScenarioMaterialSeal()
            });
    }
}
