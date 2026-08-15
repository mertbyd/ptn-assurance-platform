using System;
using NSubstitute;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Models.Catalog;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Catalog;

// islevi: Cron zamanlamasinin dogrulanmasini, vade hesabini ve surumler arasi tasinmasini dogrular.
// sistemdeki gorevi: "Gece 02:00" gibi takvim ifadelerinin ve tek vadeli surum kuralinin regresyon kapisidir.
public class ScenarioScheduleManagerTests
{
    // Bu testlerdeki kurallar veritabanina gitmez; kalicilik sinirinin davranisi kapsam disidir.
    private readonly ScenarioScheduleManager _manager =
        new(Substitute.For<ITestScenarioRepository>());

    // Gece 02:00 ifadesi bir sonraki 02:00'a denk gelen UTC vadesini uretmelidir.
    [Fact]
    public void Nightly_cron_should_produce_the_next_utc_occurrence()
    {
        var now = new DateTime(2026, 8, 15, 23, 0, 0, DateTimeKind.Utc);

        var next = ScenarioScheduleManager.ComputeNextRunAt("0 2 * * *", now);

        next.ShouldBe(new DateTime(2026, 8, 16, 2, 0, 0, DateTimeKind.Utc));
    }

    // Zamanlama acikken cron verilmezse istek kararli kodla reddedilmelidir.
    [Fact]
    public void Enabling_a_schedule_without_a_cron_should_be_rejected()
    {
        var scenario = CreateScenario();

        var exception = Should.Throw<BusinessException>(() => _manager.Apply(
            scenario,
            new TestScenarioScheduleModel { ScheduleEnabled = true },
            DateTime.UtcNow));

        exception.Code.ShouldBe(TestModuleScenarioErrorCodes.ScheduleCronRequired);
    }

    // Ayristirilamayan cron ifadesi kararli kodla reddedilmelidir.
    [Fact]
    public void An_unparsable_cron_should_be_rejected()
    {
        var scenario = CreateScenario();

        var exception = Should.Throw<BusinessException>(() => _manager.Apply(
            scenario,
            new TestScenarioScheduleModel { ScheduleEnabled = true, ScheduleCron = "not a cron" },
            DateTime.UtcNow));

        exception.Code.ShouldBe(TestModuleScenarioErrorCodes.ScheduleCronInvalid);
    }

    // Zamanlama kapatildiginda cron ve vade birlikte temizlenmelidir.
    [Fact]
    public void Disabling_a_schedule_should_clear_the_cron_and_the_due_date()
    {
        var scenario = CreateScenario();
        _manager.Apply(
            scenario,
            new TestScenarioScheduleModel { ScheduleEnabled = true, ScheduleCron = "0 2 * * *" },
            DateTime.UtcNow);

        _manager.Apply(scenario, new TestScenarioScheduleModel { ScheduleEnabled = false }, DateTime.UtcNow);

        scenario.ScheduleCron.ShouldBeNull();
        scenario.ScheduleEnabled.ShouldBeFalse();
        scenario.NextRunAt.ShouldBeNull();
    }

    // Yeni surum yayinlandiginda zamanlama tasinmali ve onceki surum vadesiz kalmalidir.
    [Fact]
    public void Publishing_a_new_version_should_move_the_schedule_off_the_previous_version()
    {
        var previous = CreateScenario();
        var current = CreateScenario();
        _manager.Apply(
            previous,
            new TestScenarioScheduleModel { ScheduleEnabled = true, ScheduleCron = "0 2 * * *" },
            DateTime.UtcNow);

        ScenarioScheduleManager.Transfer(previous, current);

        current.ScheduleEnabled.ShouldBeTrue();
        current.ScheduleCron.ShouldBe("0 2 * * *");
        current.NextRunAt.ShouldNotBeNull();
        previous.ScheduleEnabled.ShouldBeFalse();
        previous.NextRunAt.ShouldBeNull();
    }

    // Ayni vade iki kez tarandiginda tetikleyici referansi degismemeli, farkli vadede degismelidir.
    [Fact]
    public void Trigger_reference_should_be_stable_per_occurrence()
    {
        var scenarioId = Guid.NewGuid();
        var due = new DateTime(2026, 8, 16, 2, 0, 0, DateTimeKind.Utc);

        var first = ScenarioScheduleManager.CreateTriggerRef(scenarioId, due);
        var repeated = ScenarioScheduleManager.CreateTriggerRef(scenarioId, due);
        var next = ScenarioScheduleManager.CreateTriggerRef(scenarioId, due.AddDays(1));

        repeated.ShouldBe(first);
        next.ShouldNotBe(first);
    }

    // Testler icin davranis calistirmayan bos senaryo kabugu uretir.
    private static TestScenario CreateScenario()
    {
        return new TestScenario(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            null,
            new TestScenarioCreateModel
            {
                ScenarioKey = "checkout.happy-path",
                Title = "Checkout",
                SourceDocument = "arazzo: 1.0.1",
                SourceHash = new string('a', 64),
                MaterialSeal = new TestScenarioMaterialSeal()
            });
    }
}
