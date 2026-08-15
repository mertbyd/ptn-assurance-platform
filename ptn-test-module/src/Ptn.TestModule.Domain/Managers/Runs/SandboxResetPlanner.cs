using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Sandbox reset ayarini dogrular ve ortama ozel ayri connection-string adini uretir.
// sistemdeki gorevi: Rollback stratejisini kosum hattindan uzak tutar; Application servisini ayar ve isim kararlarindan arindirir.
/// <summary>
/// Test verisi sandbox'inin dogrulanmis reset planini uretir.
/// </summary>
public class SandboxResetPlanner : TestModuleDomainService
{
    /// <summary>Tenant-aware sandbox reset ayarini cozen provider'dir.</summary>
    private readonly ISettingProvider _settingProvider;

    // Sandbox stratejisini aktif ABP setting provider'a baglar.
    /// <summary>Planner'i aktif setting provider ile kurar.</summary>
    public SandboxResetPlanner(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // Rollback'i ve bilinmeyen stratejileri reddedip ortama ozel ayri baglanti planini kurar.
    /// <summary>Verilen ortam icin desteklenen sandbox reset planini uretir.</summary>
    public async Task<SandboxResetPlan> CreatePlanAsync(
        string environmentKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureEnvironmentKeyIsSafe(environmentKey);
        var configuredStrategy = await _settingProvider.GetOrNullAsync(
            TestModuleRunSettingNames.SandboxResetStrategy);
        var strategy = configuredStrategy?.Trim() ?? TestModuleRunSettingNames.DefaultSandboxResetStrategy;
        EnsureStrategyIsSupported(strategy);

        return new SandboxResetPlan
        {
            StrategyCode = TestModuleRunSettingNames.RespawnResetStrategy,
            ConnectionStringName = string.Format(
                CultureInfo.InvariantCulture,
                TestModuleRunSettingNames.SandboxConnectionStringNameFormat,
                environmentKey)
        };
    }

    // Ortam anahtarinin configuration yoluna yeni bolum ekleyemeyecek bicimde oldugunu dogrular.
    /// <summary>Ortam anahtarinin connection-string adi icin guvenli oldugunu dogrular.</summary>
    private static void EnsureEnvironmentKeyIsSafe(string environmentKey)
    {
        if (string.IsNullOrWhiteSpace(environmentKey) ||
            !Regex.IsMatch(environmentKey, TestModuleRunSettingNames.SandboxEnvironmentKeyPattern))
        {
            throw new BusinessException(TestModuleRunErrorCodes.SandboxEnvironmentKeyInvalid)
                .WithData(nameof(environmentKey), environmentKey);
        }
    }

    // SUT kendi transaction'ini actigi icin rollback dahil desteklenmeyen stratejileri reddeder.
    /// <summary>Yalniz iliskisel Respawn temizleme stratejisini kabul eder.</summary>
    private static void EnsureStrategyIsSupported(string strategy)
    {
        if (!string.Equals(
                strategy,
                TestModuleRunSettingNames.RespawnResetStrategy,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(TestModuleRunErrorCodes.SandboxResetStrategyUnsupported)
                .WithData(nameof(strategy), strategy);
        }
    }
}
