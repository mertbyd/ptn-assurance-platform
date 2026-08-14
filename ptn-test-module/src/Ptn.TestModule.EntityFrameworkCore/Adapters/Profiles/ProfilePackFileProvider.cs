using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Documents.Bridge.Profiles;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Mappers.Bridge.Profiles;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;
using Volo.Abp.Settings;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ptn.TestModule.Adapters;

// islevi: Profil paketini ayarli dosya kokunden guvenli bicimde okur, YAML'i cevirir ve SHA-256 ile muhurlar.
// sistemdeki gorevi: Domain profil portunu tablo veya yeni katman acmadan dosya tabanli adapter olarak uygular.
public class ProfilePackFileProvider : IProfilePackProvider
{
    private static readonly ProfilePackMapper Mapper = new();
    private readonly ISettingProvider _settingProvider;
    private readonly IHostEnvironment _hostEnvironment;

    // Ayar ve host kokunu guvenli profil dosyasi cozumlemesi icin baglar.
    public ProfilePackFileProvider(
        ISettingProvider settingProvider,
        IHostEnvironment hostEnvironment)
    {
        _settingProvider = settingProvider;
        _hostEnvironment = hostEnvironment;
    }

    // Profil dosyasini boyut butcesiyle okur, semasini cevirir ve icerik fingerprint'ini ekler.
    public async Task<PtnProfilePack> LoadAsync(
        string profileKey,
        CancellationToken cancellationToken)
    {
        EnsureProfileKeyIsSafe(profileKey);
        var filePath = await ResolveFilePathAsync(profileKey);
        var bytes = await ReadProfileBytesAsync(filePath, cancellationToken);
        var document = Deserialize(bytes);
        var pack = Mapper.Map(document);
        EnsureProfileMatchesRequest(pack, profileKey);
        pack.ContentFingerprint = ComputeFingerprint(bytes);
        return pack;
    }

    // Ayarli kok yolu host kokune baglar ve profil dosyasinin kok disina cikmasini engeller.
    private async Task<string> ResolveFilePathAsync(string profileKey)
    {
        var configured = await _settingProvider.GetOrNullAsync(PtnBridgeSettingNames.ProfilePackPath)
                         ?? PtnBridgeSettingNames.DefaultProfilePackPath;
        var root = Path.IsPathRooted(configured)
            ? Path.GetFullPath(configured)
            : Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configured));
        var filePath = Path.GetFullPath(Path.Combine(
            root,
            profileKey + PtnBridgeSettingNames.ProfilePackExtension));
        var rootedPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!filePath.StartsWith(rootedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }

        return filePath;
    }

    // Dosyayi tek handle uzerinden okur ve okumadan once profil boyut butcesini uygular.
    private static async Task<byte[]> ReadProfileBytesAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackNotFound);
        }

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length <= 0 || stream.Length > PtnBridgeConsts.MaxProfilePackBytes)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }

        var bytes = new byte[(int)stream.Length];
        await stream.ReadExactlyAsync(bytes, cancellationToken);
        return bytes;
    }

    // YAML belgesini bilinmeyen alanlari reddeden dar transport modeline cevirir.
    private static ProfilePackDocument Deserialize(byte[] bytes)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            using var reader = new StreamReader(new MemoryStream(bytes));
            return deserializer.Deserialize<ProfilePackDocument>(reader);
        }
        catch (YamlException exception)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid, innerException: exception);
        }
    }

    // Profil anahtarini yalniz dosya-adi guvenli kapali karakterlerle sinirlar.
    private static void EnsureProfileKeyIsSafe(string profileKey)
    {
        var safe = !string.IsNullOrWhiteSpace(profileKey) && profileKey.All(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.');
        if (!safe)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }
    }

    // Dosya icerigi ile YAML icindeki profil anahtarinin ayni kaydi gostermesini zorunlu kilar.
    private static void EnsureProfileMatchesRequest(PtnProfilePack pack, string profileKey)
    {
        if (pack.ProfileKey != profileKey)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }
    }

    // Ham profil baytlarini lowercase sha256: sozlesmesine cevirir.
    private static string ComputeFingerprint(byte[] bytes)
    {
        return PtnBridgeSettingNames.FingerprintPrefix +
               Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

}
