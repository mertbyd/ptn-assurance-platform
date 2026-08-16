using System.Threading.Tasks;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.DatabaseChecker.Managers.Assertions;

// islevi: Tenant -> global -> default zincirinden assertion limitlerini okuyup pozitif bir calisma profiline cevirir.
// sistemdeki gorevi: Polling, regex, row ve batch guvenlik tavanlarinin her public ucta ayni sekilde uygulanmasini saglar.
public class AssertionSettingsResolver : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    // islevi: Resolver'i ABP'nin tenant-aware setting okuyucusuyla kurar.
    public AssertionSettingsResolver(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // islevi: Tum assertion limitlerini tek tenant-aware calisma profili olarak cozer.
    public async Task<AssertionExecutionSettings> ResolveAsync()
    {
        return new AssertionExecutionSettings
        {
            MaxTimeoutMs = await ReadPositiveAsync(DatabaseCheckerSettings.Assertion.MaxTimeoutMs, DatabaseCheckerSettings.Assertion.DefaultMaxTimeoutMs),
            MinPollIntervalMs = await ReadPositiveAsync(DatabaseCheckerSettings.Assertion.MinPollIntervalMs, DatabaseCheckerSettings.Assertion.DefaultMinPollIntervalMs),
            MaxRowsPerAssertion = await ReadPositiveAsync(DatabaseCheckerSettings.Assertion.MaxRowsPerAssertion, DatabaseCheckerSettings.Assertion.DefaultMaxRowsPerAssertion),
            RegexTimeoutMs = await ReadPositiveAsync(DatabaseCheckerSettings.Assertion.RegexTimeoutMs, DatabaseCheckerSettings.Assertion.DefaultRegexTimeoutMs),
            MaxBatchSize = await ReadPositiveAsync(DatabaseCheckerSettings.Assertion.MaxBatchSize, DatabaseCheckerSettings.Assertion.DefaultMaxBatchSize)
        };
    }

    // islevi: Tek integer setting'i fallback degeriyle okuyup pozitiflik kuralini uygular.
    private async Task<int> ReadPositiveAsync(string name, int defaultValue)
    {
        var value = await _settingProvider.GetAsync(name, defaultValue);
        if (value <= 0)
        {
            throw new BusinessException(AssertionExceptionCodes.InvalidSetting).WithData("SettingName", name);
        }

        return value;
    }
}
