using System;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Tenant ortam ayarinin bulunma ve API-DB environmentKey eslesme kapilarini dogrular.
// sistemdeki gorevi: Baglanmamis veya staging-prod karisik ortamin kosumu baslatmasini engeller.
/// <summary>
/// RunEnvironmentBindingManager ortam cozumleme testleridir.
/// </summary>
public class RunEnvironmentBindingTests
{
    // Tenant setting'inde bulunmayan ortam kararli EnvironmentNotBound hatasi vermelidir.
    /// <summary>Baglanmamis mantiksal ortamin reddedildigini dogrular.</summary>
    [Fact]
    public async Task Should_reject_unbound_environment()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(TestModuleRunSettingNames.EnvironmentBindings).Returns("{}");
        var manager = new RunEnvironmentBindingManager(settings);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.ResolveAsync("staging"));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.EnvironmentNotBound);
    }

    // API staging ile database production eslesmesi kosumdan once reddedilmelidir.
    /// <summary>API ve database environmentKey uyusmazliginin reddedildigini dogrular.</summary>
    [Fact]
    public async Task Should_reject_api_and_database_environment_mismatch()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(TestModuleRunSettingNames.EnvironmentBindings)
            .Returns(CreateBindingJson("staging", "production"));
        var manager = new RunEnvironmentBindingManager(settings);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.ResolveAsync("staging"));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.EnvironmentMismatch);
    }

    // Iki taraf da istenen anahtari tasidiginda dogrulanmis binding donmelidir.
    /// <summary>Eslesen ortam ayarinin guvenli binding modeline cozuldugunu dogrular.</summary>
    [Fact]
    public async Task Should_resolve_matching_environment_binding()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(TestModuleRunSettingNames.EnvironmentBindings)
            .Returns(CreateBindingJson("staging", "staging"));
        var manager = new RunEnvironmentBindingManager(settings);

        var binding = await manager.ResolveAsync("staging");

        binding.EnvironmentKey.ShouldBe("staging");
        binding.BaseUrl.ShouldBe("https://staging.example.test");
        binding.SecretRef.ShouldBe("vault/staging");
    }

    // Test ortam haritasini iki acik environmentKey kaydiyla JSON olarak kurar.
    /// <summary>API ve database anahtarlari ayarlanabilir tenant binding JSON'u dondurur.</summary>
    private static string CreateBindingJson(string apiEnvironmentKey, string databaseEnvironmentKey)
    {
        return $$"""
                 {
                   "staging": {
                     "api": {
                       "environmentKey": "{{apiEnvironmentKey}}",
                       "baseUrl": "https://staging.example.test",
                       "specSnapshotId": "{{Guid.NewGuid()}}"
                     },
                     "database": {
                       "environmentKey": "{{databaseEnvironmentKey}}",
                       "dbConnectionId": "{{Guid.NewGuid()}}",
                       "secretRef": "vault/staging"
                     }
                   }
                 }
                 """;
    }
}
