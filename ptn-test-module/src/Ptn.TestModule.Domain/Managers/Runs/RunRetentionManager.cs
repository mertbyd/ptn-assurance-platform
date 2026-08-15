using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using Volo.Abp.Settings;
using Volo.Abp.Timing;

namespace Ptn.TestModule.Managers.Runs;

// islevi: HAR ve tamamlanmis kosum saklama ayarlarini dogrular, purge dilimini ve kalici islemleri yonetir.
// sistemdeki gorevi: Background job'i business validation, zaman esigi ve repository kararlarindan arindirir.
/// <summary>Tamamlanmis kosumlar icin setting-driven parcali saklama politikasini uygular.</summary>
public class RunRetentionManager : TestModuleDomainService
{
    private readonly ITestRunRepository _repository;
    private readonly ISettingProvider _settingProvider;
    private readonly IClock _clock;

    /// <summary>Manager'i kosum repository'si, ABP setting provider ve saat ile kurar.</summary>
    public RunRetentionManager(
        ITestRunRepository repository,
        ISettingProvider settingProvider,
        IClock clock)
    {
        _repository = repository;
        _settingProvider = settingProvider;
        _clock = clock;
    }

    /// <summary>Aktif tenant ayarlarindan HAR ve kosum purge esiklerini olusturur.</summary>
    public async Task<RunRetentionPlan> CreatePlanAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var harDays = ParsePositiveSetting(
            await _settingProvider.GetOrNullAsync(TestModuleRunSettingNames.HarRetentionDays),
            TestModuleRunSettingNames.DefaultHarRetentionDays,
            TestModuleRunSettingNames.HarRetentionDays);
        var runDays = ParsePositiveSetting(
            await _settingProvider.GetOrNullAsync(TestModuleRunSettingNames.RunRetentionDays),
            TestModuleRunSettingNames.DefaultRunRetentionDays,
            TestModuleRunSettingNames.RunRetentionDays);
        var batchSize = ParsePositiveSetting(
            await _settingProvider.GetOrNullAsync(TestModuleRunSettingNames.RunPurgeBatchSize),
            TestModuleRunSettingNames.DefaultRunPurgeBatchSize,
            TestModuleRunSettingNames.RunPurgeBatchSize);

        return new RunRetentionPlan
        {
            HarCompletedBefore = _clock.Now.AddDays(-harDays),
            RunCompletedBefore = _clock.Now.AddDays(-runDays),
            BatchSize = batchSize
        };
    }

    /// <summary>HAR suresi dolmus tamamlanmis kosumlarin bir dilimini getirir.</summary>
    public Task<IReadOnlyList<TestRun>> GetExpiredHarArtifactsAsync(
        RunRetentionPlan plan,
        CancellationToken cancellationToken = default)
        => _repository.GetExpiredHarArtifactsAsync(plan.HarCompletedBefore, plan.BatchSize, cancellationToken);

    /// <summary>Blob'u silinen kosumlarin HAR referanslarini topluca temizler.</summary>
    public Task ClearHarArtifactNamesAsync(
        IReadOnlyCollection<TestRun> runs,
        CancellationToken cancellationToken = default)
        => _repository.ClearHarArtifactNamesAsync(runs.Select(entity => entity.Id).ToArray(), cancellationToken);

    /// <summary>Saklama suresi dolmus tamamlanmis kosumlarin bir dilimini getirir.</summary>
    public Task<IReadOnlyList<TestRun>> GetExpiredRunsAsync(
        RunRetentionPlan plan,
        CancellationToken cancellationToken = default)
        => _repository.GetExpiredRunsAsync(plan.RunCompletedBefore, plan.BatchSize, cancellationToken);

    /// <summary>Saklama suresi dolmus kosumlari sonuc ve bulgu cascade'iyle topluca siler.</summary>
    public Task DeleteExpiredRunsAsync(
        IReadOnlyCollection<TestRun> runs,
        CancellationToken cancellationToken = default)
        => _repository.DeleteExpiredRunsAsync(runs.Select(entity => entity.Id).ToArray(), cancellationToken);

    private static int ParsePositiveSetting(string? configuredValue, string defaultValue, string settingName)
    {
        var value = configuredValue ?? defaultValue;
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            throw new BusinessException(TestModuleRunErrorCodes.RunRetentionSettingInvalid)
                .WithData(nameof(settingName), settingName)
                .WithData(nameof(configuredValue), configuredValue ?? string.Empty);
        }

        return parsed;
    }
}
