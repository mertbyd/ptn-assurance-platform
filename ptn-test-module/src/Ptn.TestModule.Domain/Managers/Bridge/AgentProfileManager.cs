using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Tenant-scoped ajan profillerini ABP Setting zincirinden cozer.
// sistemdeki gorevi: Alti anin tool, tur ve token tavanlarini tek domain sahibinde tipler.
public class AgentProfileManager : TestModuleDomainService
{
    private readonly ISettingProvider _settingProvider;
    public AgentProfileManager(ISettingProvider settingProvider) => _settingProvider = settingProvider;

    // Tek moment profilini aktif tenant setting degerlerinden cozer.
    public async Task<AgentProfile> ResolveAsync(string momentCode, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tools = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.AllowedTools(momentCode)) ??
                    PtnBridgeSettingNames.DefaultAllowedTools(momentCode);
        var maxTurns = await _settingProvider.GetAsync<int>(PtnBridgeSettingNames.MaxTurns(momentCode));
        var tokenLimit = await _settingProvider.GetAsync<int>(PtnBridgeSettingNames.TokenLimit(momentCode));
        return new AgentProfile
        {
            MomentCode = momentCode,
            AllowedToolCodes = tools.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            MaxTurns = maxTurns,
            TokenLimit = tokenLimit
        };
    }
}
