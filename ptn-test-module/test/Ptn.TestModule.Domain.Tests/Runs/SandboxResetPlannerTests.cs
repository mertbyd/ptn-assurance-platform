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

// islevi: Sandbox reset stratejisi ve ayri ortam baglantisi kararlarini dogrular.
// sistemdeki gorevi: Rollback'in kosum hattina girmesini ve checker hedef baglantisinin yeniden kullanilmasini engeller.
public class SandboxResetPlannerTests
{
    // Her ortam checker baglantisindan ayri ve kendine ozel connection-string adini almalidir.
    [Fact]
    public async Task Should_create_a_dedicated_connection_name_for_each_environment()
    {
        var plan = await CreatePlanner().CreatePlanAsync("staging-eu");

        plan.StrategyCode.ShouldBe(TestModuleRunSettingNames.RespawnResetStrategy);
        plan.ConnectionStringName.ShouldBe("TestModuleSandbox.staging-eu");
        plan.ConnectionStringName.ShouldNotBe("Default");
    }

    // SUT kendi baglantisini actigi icin transaction rollback reset stratejisi kabul edilmemelidir.
    [Fact]
    public async Task Should_reject_transaction_rollback_strategy()
    {
        var planner = CreatePlanner("Rollback");

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            planner.CreatePlanAsync("staging"));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.SandboxResetStrategyUnsupported);
    }

    // Configuration yolunu degistirebilecek ortam anahtari ayri baglanti adina tasinmamalidir.
    [Fact]
    public async Task Should_reject_unsafe_environment_key()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            CreatePlanner().CreatePlanAsync("staging:Default"));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.SandboxEnvironmentKeyInvalid);
    }

    // Istenen stratejiyi veya varsayilani donduren setting provider ile planner kurar.
    private static SandboxResetPlanner CreatePlanner(string? strategy = null)
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(TestModuleRunSettingNames.SandboxResetStrategy)
            .Returns(strategy);
        return new SandboxResetPlanner(settingProvider);
    }
}
