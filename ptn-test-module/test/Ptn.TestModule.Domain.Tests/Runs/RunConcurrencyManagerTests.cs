using System;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Kosum kilidinin tenant-ortam ayrimini, bekleme butcesini ve timeout kararini dogrular.
// sistemdeki gorevi: Ayni ortam kosumlarini siraya alirken farkli ortamlarin birbirini beklemesini engeller.
public class RunConcurrencyManagerTests
{
    // Ayni tenant ve ortam her teslimde ayni kilidi kullanip pozitif sure beklemelidir.
    [Fact]
    public async Task Should_queue_runs_for_the_same_environment_on_the_same_lock()
    {
        var tenantId = Guid.NewGuid();
        var manager = CreateManager("45");

        var first = await manager.CreatePlanAsync(tenantId, "staging");
        var second = await manager.CreatePlanAsync(tenantId, "staging");

        first.LockName.ShouldBe(second.LockName);
        first.WaitTimeout.ShouldBe(TimeSpan.FromSeconds(45));
    }

    // Farkli ortamlar birbirini beklememek icin farkli ABP lock anahtarlari almalidir.
    [Fact]
    public async Task Should_isolate_different_environments()
    {
        var manager = CreateManager();
        var tenantId = Guid.NewGuid();

        var staging = await manager.CreatePlanAsync(tenantId, "staging");
        var production = await manager.CreatePlanAsync(tenantId, "production");

        staging.LockName.ShouldNotBe(production.LockName);
        staging.LockName.ShouldContain(tenantId.ToString("N"));
        staging.LockName.ShouldEndWith(":staging");
    }

    // Sifir bekleme ABP'nin try-only davranisidir ve siraya alma kabulunu saglamaz.
    [Fact]
    public async Task Should_reject_non_positive_wait_timeout()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            CreateManager("0").CreatePlanAsync(null, "staging"));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.RunConcurrencyWaitInvalid);
    }

    // Istenen bekleme degerini donduren setting provider ile Manager kurar.
    private static RunConcurrencyManager CreateManager(string? waitSeconds = null)
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(TestModuleRunSettingNames.RunConcurrencyWaitSeconds)
            .Returns(waitSeconds);
        return new RunConcurrencyManager(settingProvider);
    }
}
