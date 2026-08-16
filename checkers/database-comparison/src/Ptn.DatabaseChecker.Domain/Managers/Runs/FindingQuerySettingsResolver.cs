using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Runs;
using Ptn.DatabaseChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.DatabaseChecker.Managers.Runs;

// islevi: Tenant -> global -> default zincirinden bulgu sayfalama ve cevap butcesi limitlerini cozer.
// sistemdeki gorevi: MCP'nin kucuk parca okuma tavanlarini controller ve repository kodundan ayri tek setting sahibinde tutar.
/// <summary>
/// Bulgu sorgu limitlerini ABP tenant, global ve varsayilan ayar zincirinden cozer.
/// </summary>
public class FindingQuerySettingsResolver : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    /// <summary>
    /// Resolver'i ABP'nin tenant-aware setting okuyucusuyla kurar.
    /// </summary>
    public FindingQuerySettingsResolver(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    /// <summary>
    /// Bulgu sorgusunun sayfa ve UTF-8 cevap butcesi profilini cozer.
    /// </summary>
    public async Task<FindingQuerySettings> ResolveAsync()
    {
        var maxPageSize = await ReadPositiveAsync(
            DatabaseCheckerSettings.Findings.MaxPageSize,
            DatabaseCheckerSettings.Findings.DefaultMaxPageSize);
        var defaultPageSize = await ReadPositiveAsync(
            DatabaseCheckerSettings.Findings.PageSize,
            DatabaseCheckerSettings.Findings.DefaultPageSize);
        return new FindingQuerySettings
        {
            DefaultPageSize = Math.Min(defaultPageSize, maxPageSize),
            MaxPageSize = maxPageSize,
            MaxResponseBytes = await ReadPositiveAsync(
                DatabaseCheckerSettings.Findings.MaxResponseBytes,
                DatabaseCheckerSettings.Findings.DefaultMaxResponseBytes)
        };
    }

    // islevi: Tek integer setting'i fallback degeriyle okuyup pozitiflik kuralini uygular.
    private async Task<int> ReadPositiveAsync(string name, int defaultValue)
    {
        var value = await _settingProvider.GetAsync(name, defaultValue);
        if (value <= 0)
        {
            throw new BusinessException(ComparisonRunExceptionCodes.InvalidFindingSetting)
                .WithData("SettingName", name);
        }

        return value;
    }
}
