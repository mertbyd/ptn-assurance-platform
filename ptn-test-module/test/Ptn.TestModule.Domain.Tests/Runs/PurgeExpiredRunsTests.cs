using System;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Retention ayarlarinin 90 gunluk varsayilanini, batch sinirini ve bozuk ayar reddini dogrular.
// sistemdeki gorevi: Purge job'inin aktif kosumlara dokunmadan Manager'in urettigi esikleri kullanmasini sabitler.
public class PurgeExpiredRunsTests
{
    [Fact]
    public async Task Should_default_har_and_run_retention_to_ninety_days()
    {
        var now = new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
        var manager = CreateManager(now);

        var plan = await manager.CreatePlanAsync();

        plan.HarCompletedBefore.ShouldBe(now.AddDays(-90));
        plan.RunCompletedBefore.ShouldBe(now.AddDays(-90));
        plan.BatchSize.ShouldBe(10000);
    }

    [Fact]
    public async Task Should_honor_independent_blob_and_run_ttl_settings()
    {
        var now = new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
        var manager = CreateManager(now, "30", "120", "250");

        var plan = await manager.CreatePlanAsync();

        plan.HarCompletedBefore.ShouldBe(now.AddDays(-30));
        plan.RunCompletedBefore.ShouldBe(now.AddDays(-120));
        plan.BatchSize.ShouldBe(250);
    }

    [Fact]
    public async Task Should_reject_non_positive_retention_settings()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            CreateManager(DateTime.UtcNow, "0").CreatePlanAsync());

        exception.Code.ShouldBe(TestModuleRunErrorCodes.RunRetentionSettingInvalid);
    }

    private static RunRetentionManager CreateManager(
        DateTime now,
        string? harDays = null,
        string? runDays = null,
        string? batchSize = null)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(TestModuleRunSettingNames.HarRetentionDays).Returns(harDays);
        settings.GetOrNullAsync(TestModuleRunSettingNames.RunRetentionDays).Returns(runDays);
        settings.GetOrNullAsync(TestModuleRunSettingNames.RunPurgeBatchSize).Returns(batchSize);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(now);

        return new RunRetentionManager(Substitute.For<ITestRunRepository>(), settings, clock);
    }
}
