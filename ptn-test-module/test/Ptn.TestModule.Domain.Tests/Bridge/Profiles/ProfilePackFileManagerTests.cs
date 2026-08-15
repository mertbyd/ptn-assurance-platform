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
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Domain.Tests.Bridge.Profiles;

// islevi: YAML profil manager'inin kok siniri, sema cevirisi ve SHA-256 muhrunu dogrular.
// sistemdeki gorevi: Profil dosyasi okumasinin traversal, boyut ve kimlik hatalarina acik olmamasini saglar.
public class ProfilePackFileManagerTests
{
    // Gecerli YAML profilini domain modeline cevirip sha256 icerik fingerprint'i ekler.
    [Fact]
    public async Task Should_load_and_fingerprint_profile_pack()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "unit.yaml"), ValidProfile, Encoding.UTF8);
            var provider = CreateProvider(root);

            var pack = await provider.LoadAsync("unit", CancellationToken.None);

            pack.ProfileKey.ShouldBe("unit");
            pack.Paths.Count.ShouldBe(1);
            pack.Paths[0].Steps[0].NodeKindCode.ShouldBe("ScopeRequired");
            pack.ContentFingerprint.ShouldStartWith("sha256:");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Profil anahtarinda yol karakteri kullanilarak ayarli kokun disina cikilmasini reddeder.
    [Fact]
    public async Task Should_reject_unsafe_profile_key()
    {
        var root = CreateTemporaryDirectory();
        try
        {
            var provider = CreateProvider(root);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                provider.LoadAsync("../secret", CancellationToken.None));

            exception.Code.ShouldBe(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    // Teste ozel ayar ve host kokunu gercek manager'a verir.
    private static ProfilePackFileManager CreateProvider(string profileRoot)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(PtnBridgeSettingNames.ProfilePackPath).Returns(profileRoot);
        var host = Substitute.For<IHostEnvironment>();
        host.ContentRootPath.Returns(profileRoot);
        return new ProfilePackFileManager(settings, host);
    }

    // Her test icin kullanici verisinden bagimsiz benzersiz profil klasoru olusturur.
    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ptn-bridge-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private const string ValidProfile = """
profileKey: unit
revision: "1"
dbSchemaFingerprint: "sha256:unit"
bindings:
  - conceptCode: Subject
    dbSchemaName: identity
    tableName: users
    columnMap: { identity: id }
    patternCode: SE
    stateCode: Approved
paths:
  - pathKey: access-denied-403
    trigger: { statusCodes: [403] }
    steps:
      - nodeKind: ScopeRequired
        source: api.failureIdentity
    confirmedWhen: "ScopeRequired.observed"
    inconclusiveWhen: "any(step.state == Unavailable)"
""";
}
