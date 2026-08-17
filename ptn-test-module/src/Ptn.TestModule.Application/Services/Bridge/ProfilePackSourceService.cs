using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Profil paketi kokune yazma ve yuklu anahtarlari listeleme dosya sinirini uygular.
// sistemdeki gorevi: Ad, butce ve kok kararlarini Manager'da birakip yalniz dosya sistemi mekanigini tasir.
[ExposeServices(typeof(IProfilePackSourcePort))]
public sealed class ProfilePackSourceService : IProfilePackSourcePort, ITransientDependency
{
    private readonly ProfilePackFileManager _manager;
    private readonly ISettingProvider _settingProvider;
    private readonly IHostEnvironment _hostEnvironment;

    // Profil dosyasi yazimini mevcut anahtar, butce ve kok kapilarina baglar.
    public ProfilePackSourceService(
        ProfilePackFileManager manager,
        ISettingProvider settingProvider,
        IHostEnvironment hostEnvironment)
    {
        _manager = manager;
        _settingProvider = settingProvider;
        _hostEnvironment = hostEnvironment;
    }

    // Anahtardan dosya adini turetir, Manager kapilarindan gecirir ve ayarli koke yazar.
    public async Task WriteAsync(string profileKey, byte[] content, CancellationToken cancellationToken = default)
    {
        _manager.EnsureProfileKeyIsSafe(profileKey);
        _manager.EnsureWithinBudget(content.LongLength);
        var configured = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.ProfilePackPath)
                         ?? PtnBridgeSettingNames.DefaultProfilePackPath;
        var root = Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configured));
        var filePath = Path.GetFullPath(
            Path.Combine(root, profileKey + PtnBridgeSettingNames.ProfilePackExtension));
        _manager.EnsureSourceIsWithinRoot(
            root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            filePath);
        Directory.CreateDirectory(root);
        await File.WriteAllBytesAsync(filePath, content, cancellationToken);
    }

    // Ayarli kokteki paket dosyalarini kararli sirada anahtar listesine cevirir.
    public async Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken cancellationToken = default)
    {
        var configured = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.ProfilePackPath)
                         ?? PtnBridgeSettingNames.DefaultProfilePackPath;
        var root = Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configured));
        if (!Directory.Exists(root))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(root, "*" + PtnBridgeSettingNames.ProfilePackExtension, SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key!)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();
    }
}
