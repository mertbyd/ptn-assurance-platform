using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Ajan politikasi belgesini ayarli kokten okuyup ham baytlarini port sozlesmesine uygular.
// sistemdeki gorevi: Politikayi assembly'ye gomulu kopyadan kurtarip is kurallariyla ayni dosya sinirina baglar.
[ExposeServices(typeof(IAgentPolicySourcePort))]
public sealed class AgentPolicySourceService : IAgentPolicySourcePort, ITransientDependency
{
    private readonly BusinessRuleFingerprintManager _manager;
    private readonly ISettingProvider _settingProvider;
    private readonly IHostEnvironment _hostEnvironment;

    // Politika okumasini mevcut adreslenebilirlik ve butce kapisina baglar.
    public AgentPolicySourceService(
        BusinessRuleFingerprintManager manager,
        ISettingProvider settingProvider,
        IHostEnvironment hostEnvironment)
    {
        _manager = manager;
        _settingProvider = settingProvider;
        _hostEnvironment = hostEnvironment;
    }

    // Ayarli kok yolunu cozer, kaynagi Manager kapisindan gecirir ve tek handle uzerinden okur.
    public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
    {
        var configured = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.AgentPolicyPath)
                         ?? PtnBridgeSettingNames.DefaultAgentPolicyPath;
        var root = Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configured));
        var filePath = Path.GetFullPath(Path.Combine(root, PtnBridgeSettingNames.AgentPolicyFileName));
        _manager.EnsureSourceIsAddressable(
            root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            filePath,
            File.Exists(filePath));
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        _manager.EnsureWithinBudget(stream.Length);
        var bytes = new byte[(int)stream.Length];
        await stream.ReadExactlyAsync(bytes, cancellationToken);
        return bytes;
    }
}
