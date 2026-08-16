using System.Globalization;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.ApiContractChecker.Managers.Runs;

// islevi: Bulgu sayfa ve byte esiklerini tenant-global-default setting zincirinden cozer.
// sistemdeki gorevi: AppService ve repository'nin inline limit tasimasini engeller.
public sealed class FindingPagePolicyResolver : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    public FindingPagePolicyResolver(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // islevi: Uc pozitif bulgu butcesini fail-closed varsayilanlarla okur.
    public async Task<FindingPagePolicy> ResolveAsync()
    {
        return new FindingPagePolicy
        {
            DefaultPageSize = await ReadPositiveAsync(
                ApiContractCheckerSettings.Findings.DefaultPageSize,
                ContractCheckRunConsts.DefaultFindingPageSize),
            MaxPageSize = await ReadPositiveAsync(
                ApiContractCheckerSettings.Findings.MaxPageSize,
                ContractCheckRunConsts.DefaultMaxFindingPageSize),
            MaxResponseBytes = await ReadPositiveAsync(
                ApiContractCheckerSettings.Findings.MaxResponseBytes,
                ContractCheckRunConsts.DefaultFindingPageMaxBytes)
        };
    }

    // islevi: Bos veya gecersiz setting degerini kararli pozitif varsayilana indirger.
    private async Task<int> ReadPositiveAsync(string name, int fallback)
    {
        var value = await _settingProvider.GetOrNullAsync(name);
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : fallback;
    }
}
