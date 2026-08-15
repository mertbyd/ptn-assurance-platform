using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Tenant ve ortam bazli kosum kilidi anahtarini, bekleme suresini ve edinme sonucunu kararlastirir.
// sistemdeki gorevi: Background job'i inline kilit adindan ve eszamanlilik business kararindan arindirir; ikinci kuyruk acmaz.
/// <summary>
/// Paylasilan test ortamlarinin kosum eszamanlilik kapisini planlar.
/// </summary>
public class RunConcurrencyManager : TestModuleDomainService
{
    /// <summary>Tenant-aware kilit bekleme ayarini cozen provider'dir.</summary>
    private readonly ISettingProvider _settingProvider;

    // Eszamanlilik ayarini aktif ABP setting provider'a baglar.
    /// <summary>Manager'i aktif setting provider ile kurar.</summary>
    public RunConcurrencyManager(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // Tenant ve ortam kimligini tek kilit anahtarina, ayari pozitif bekleme suresine cevirir.
    /// <summary>Verilen ortam icin ABP distributed lock edinme planini uretir.</summary>
    public async Task<RunConcurrencyPlan> CreatePlanAsync(
        Guid? tenantId,
        string environmentKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentKey);
        var configuredWait = await _settingProvider.GetOrNullAsync(
            TestModuleRunSettingNames.RunConcurrencyWaitSeconds);
        var waitSeconds = ParseWaitSeconds(configuredWait);

        return new RunConcurrencyPlan
        {
            LockName = string.Format(
                CultureInfo.InvariantCulture,
                TestModuleRunSettingNames.RunConcurrencyLockNameFormat,
                tenantId?.ToString("N") ?? TestModuleRunSettingNames.HostTenantLockSegment,
                environmentKey),
            WaitTimeout = TimeSpan.FromSeconds(waitSeconds)
        };
    }

    // ABP lock timeout sonucunu kosum ailesinin kararli retry edilebilir hatasina cevirir.
    /// <summary>Kilit handle'inin edinildigini dogrular.</summary>
    public void EnsureLockAcquired(bool acquired, string environmentKey)
    {
        if (!acquired)
        {
            throw new BusinessException(TestModuleRunErrorCodes.EnvironmentRunInProgress)
                .WithData(nameof(environmentKey), environmentKey);
        }
    }

    // Setting degerini pozitif saniye olarak cozer; bozuk ayari sessizce varsayilana dusurmez.
    /// <summary>Kilit bekleme ayarini pozitif saniyeye cevirir.</summary>
    private static int ParseWaitSeconds(string? configuredWait)
    {
        var value = configuredWait ?? TestModuleRunSettingNames.DefaultRunConcurrencyWaitSeconds;
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) ||
            seconds <= 0)
        {
            throw new BusinessException(TestModuleRunErrorCodes.RunConcurrencyWaitInvalid)
                .WithData(nameof(configuredWait), configuredWait ?? string.Empty);
        }

        return seconds;
    }
}
