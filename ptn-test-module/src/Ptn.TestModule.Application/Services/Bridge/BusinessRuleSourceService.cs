using System;
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

// islevi: Is kurali belgesini ayarli kokten okuyup ham baytlarini port sozlesmesine uygular.
// sistemdeki gorevi: Muhur karari Manager'da kalirken dosya, ayar ve kok siniri I/O'sunu tek sinirda toplar.
[ExposeServices(typeof(IBusinessRuleSourcePort))]
public sealed class BusinessRuleSourceService : IBusinessRuleSourcePort, ITransientDependency
{
    private readonly BusinessRuleFingerprintManager _manager;
    private readonly ISettingProvider _settingProvider;
    private readonly IHostEnvironment _hostEnvironment;

    // Saf muhur kararini ayar ve dosya sistemi sinirina baglar.
    public BusinessRuleSourceService(
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
        var configured = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.BusinessRulesPath)
                         ?? PtnBridgeSettingNames.DefaultBusinessRulesPath;
        var root = Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configured));
        var filePath = Path.GetFullPath(Path.Combine(root, PtnBridgeSettingNames.BusinessRulesFileName));
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

    // Ayni kok ve ayni dosya adina yazar; okuma yolu bir sonraki cagrida yeni baytlari gorur.
    public async Task WriteAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        var configured = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.BusinessRulesPath)
                         ?? PtnBridgeSettingNames.DefaultBusinessRulesPath;
        var root = Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configured));
        var filePath = Path.GetFullPath(Path.Combine(root, PtnBridgeSettingNames.BusinessRulesFileName));
        _manager.EnsureSourceIsWithinRoot(
            root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            filePath);
        _manager.EnsureWithinBudget(content.LongLength);
        Directory.CreateDirectory(root);
        await File.WriteAllBytesAsync(filePath, content, cancellationToken);
    }
}
