using System.Globalization;
using Ptn.ApiContractChecker.ExceptionCodes.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Tenant -> global -> default zincirinden response oracle limitlerini cozer.
// sistemdeki gorevi: Manager'in inline esik veya setting plumbing'i tasimasini engeller.
public class ConformanceSettingsResolver : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    public ConformanceSettingsResolver(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    public async Task<ConformanceLimits> ResolveAsync()
    {
        var maxViolations = await ResolvePositiveAsync(
            ApiContractCheckerSettings.Conformance.MaxViolations,
            ApiContractCheckerSettings.Conformance.DefaultMaxViolations);
        var maxBytes = await ResolvePositiveAsync(
            ApiContractCheckerSettings.Conformance.MaxResponseBytes,
            ApiContractCheckerSettings.Conformance.DefaultMaxResponseBytes);
        return new ConformanceLimits(maxViolations, maxBytes);
    }

    private async Task<int> ResolvePositiveAsync(string name, int defaultValue)
    {
        var value = await _settingProvider.GetOrNullAsync(name);
        if (value == null)
        {
            return defaultValue;
        }

        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        throw new BusinessException(ConformanceExceptionCodes.SettingsInvalid);
    }
}
