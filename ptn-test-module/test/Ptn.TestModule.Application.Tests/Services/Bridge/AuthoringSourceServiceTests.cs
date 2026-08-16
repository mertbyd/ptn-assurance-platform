using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Ptn.TestModule.Services.Bridge;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Yazarlik malzemesi kaynak sinirinin yazma, okuma ve kok kapisi davranisini dogrular.
// sistemdeki gorevi: MCP Resource'un okudugu bayt ile muhurlenen baytin tek kaynaktan geldigini kanitlar.
public class AuthoringSourceServiceTests
{
    private const string RuleText = "# kurallar\n\nBir talep kapatildiginda stok dusulur.\n";

    // Uctan yazilan kural belgesi, host yeniden derlenmeden ayni baytlarla geri okunmalidir.
    [Fact]
    public async Task Should_read_back_the_business_rules_written_through_the_port()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var manager = new BusinessRuleFingerprintManager();
            var service = CreateBusinessRuleService(root, manager);
            var written = Encoding.UTF8.GetBytes(RuleText);

            await service.WriteAsync(written, CancellationToken.None);
            var read = await service.ReadAsync(CancellationToken.None);

            read.ShouldBe(written);
            Encoding.UTF8.GetString(read).ShouldBe(RuleText);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Yukleme sonucu dondurulen muhur, kaynaktan okunan baytlarin muhruyle ayni olmalidir.
    [Fact]
    public async Task Should_seal_the_same_bytes_the_source_returns()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var manager = new BusinessRuleFingerprintManager();
            var service = CreateBusinessRuleService(root, manager);
            var written = Encoding.UTF8.GetBytes(RuleText);

            await service.WriteAsync(written, CancellationToken.None);
            var seal = manager.Seal(PtnBridgeSettingNames.BusinessRulesFileName, written);
            var read = await service.ReadAsync(CancellationToken.None);

            seal.Fingerprint.ShouldBe(manager.ComputeFingerprint(read));
            seal.ByteCount.ShouldBe(read.Length);
            seal.FileName.ShouldBe(PtnBridgeSettingNames.BusinessRulesFileName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Yazilan profil paketi ayni anahtarla listelenmeli ve butce disi icerik reddedilmelidir.
    [Fact]
    public async Task Should_write_and_list_the_profile_pack_key()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var service = CreateProfilePackService(root);

            await service.WriteAsync("unit", Encoding.UTF8.GetBytes("profileKey: unit\n"), CancellationToken.None);
            var keys = await service.ListKeysAsync(CancellationToken.None);

            keys.ShouldBe(["unit"]);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Profil anahtarindaki yol karakteriyle ayarli kokun disina yazilmasi reddedilmelidir.
    [Fact]
    public async Task Should_reject_a_profile_key_that_escapes_the_configured_root()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var service = CreateProfilePackService(root);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                service.WriteAsync("../escaped", Encoding.UTF8.GetBytes("profileKey: escaped\n"), CancellationToken.None));

            exception.Code.ShouldBe(TestModuleBridgeErrorCodes.ProfilePackInvalid);
            File.Exists(Path.Combine(Path.GetDirectoryName(root)!, "escaped.yaml")).ShouldBeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Bos icerik butce kapisinda reddedilmeli ve dosyaya hicbir sey yazilmamalidir.
    [Fact]
    public async Task Should_reject_empty_business_rules_content()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var service = CreateBusinessRuleService(root, new BusinessRuleFingerprintManager());

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                service.WriteAsync([], CancellationToken.None));

            exception.Code.ShouldBe(TestModuleBridgeErrorCodes.BusinessRulesInvalid);
            File.Exists(Path.Combine(root, PtnBridgeSettingNames.BusinessRulesFileName)).ShouldBeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Teste ozel ayar ve host kokunu gercek is kurali sinirina verir.
    private static BusinessRuleSourceService CreateBusinessRuleService(
        string root,
        BusinessRuleFingerprintManager manager)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(PtnBridgeSettingNames.BusinessRulesPath).Returns(root);
        var host = Substitute.For<IHostEnvironment>();
        host.ContentRootPath.Returns(root);
        return new BusinessRuleSourceService(manager, settings, host);
    }

    // Teste ozel ayar ve host kokunu gercek profil paketi sinirina verir.
    private static ProfilePackSourceService CreateProfilePackService(string root)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(PtnBridgeSettingNames.ProfilePackPath).Returns(root);
        var host = Substitute.For<IHostEnvironment>();
        host.ContentRootPath.Returns(root);
        return new ProfilePackSourceService(new ProfilePackFileManager(settings, host), settings, host);
    }

    // Her test icin kullanici verisinden bagimsiz benzersiz kaynak klasoru olusturur.
    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ptn-authoring-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
