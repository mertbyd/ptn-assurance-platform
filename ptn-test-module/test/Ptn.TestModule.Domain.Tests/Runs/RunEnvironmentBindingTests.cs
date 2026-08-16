using System;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
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

    // Bos ayardan yazilan baglama, ayni manager tarafindan kosum girdisi olarak geri cozulmelidir.
    /// <summary>Bos haritaya yazilan ortamin EnvironmentNotBound atmadan cozuldugunu dogrular.</summary>
    [Fact]
    public async Task Should_bind_a_new_environment_that_resolves_back()
    {
        var written = CreateManager("{}").Bind(TestModuleRunSettingNames.DefaultEnvironmentBindings, CreateBinding());
        var manager = CreateManager(written);

        var binding = await manager.ResolveAsync("staging");

        binding.EnvironmentKey.ShouldBe("staging");
        binding.BaseUrl.ShouldBe("https://staging.example.test");
        binding.SecretRef.ShouldBe("vault/staging");
    }

    // Ayni mantiksal anahtar ikinci kez baglanamaz.
    /// <summary>Zaten bagli ortam anahtarinin reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_a_duplicate_environment_key()
    {
        var manager = CreateManager("{}");
        var written = manager.Bind("{}", CreateBinding());

        var exception = Should.Throw<BusinessException>(() => manager.Bind(written, CreateBinding()));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.EnvironmentAlreadyBound);
    }

    // Runner ve checker'lar goreli adres cozemez; goreli taban adres yazilamaz.
    /// <summary>Mutlak olmayan taban adresin reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_a_relative_base_url()
    {
        var binding = CreateBinding();
        binding.BaseUrl = "/api";

        var exception = Should.Throw<BusinessException>(() => CreateManager("{}").Bind("{}", binding));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.EnvironmentBaseUrlInvalid);
    }

    // Iki checker hedefi de kimliklenmeden ortam yazilamaz.
    /// <summary>Bos snapshot kimliginin reddedildigini dogrular.</summary>
    [Fact]
    public void Should_reject_an_unidentified_target()
    {
        var binding = CreateBinding();
        binding.SpecSnapshotId = Guid.Empty;

        var exception = Should.Throw<BusinessException>(() => CreateManager("{}").Bind("{}", binding));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.EnvironmentTargetInvalid);
    }

    // Guncelleme ve silme yalniz bagli bir ortam uzerinde calisir.
    /// <summary>Bagli olmayan ortamin guncellenemedigini ve silinemedigini dogrular.</summary>
    [Fact]
    public void Should_reject_writing_an_unbound_environment()
    {
        var manager = CreateManager("{}");

        var rebind = Should.Throw<BusinessException>(() => manager.Rebind("{}", "staging", CreateBinding()));
        var unbind = Should.Throw<BusinessException>(() => manager.Unbind("{}", "staging"));

        rebind.Code.ShouldBe(TestModuleRunErrorCodes.EnvironmentNotBound);
        unbind.Code.ShouldBe(TestModuleRunErrorCodes.EnvironmentNotBound);
    }

    // Silinen ortam haritadan tamamen kalkmalidir.
    /// <summary>Bagli ortamin haritadan cikarildigini dogrular.</summary>
    [Fact]
    public async Task Should_unbind_a_bound_environment()
    {
        var manager = CreateManager("{}");
        var written = manager.Bind("{}", CreateBinding());

        var remaining = manager.Unbind(written, "staging");

        remaining.ShouldBe("{}");
        (await CreateManager(remaining).ListAsync()).ShouldBeEmpty();
    }

    // Harita her yazimdan sonra ayni anahtar sirasini tasimalidir.
    /// <summary>Yazma sirasindan bagimsiz kararli anahtar siralamasini dogrular.</summary>
    [Fact]
    public void Should_serialize_the_map_in_a_stable_key_order()
    {
        var manager = CreateManager("{}");
        var alpha = CreateBinding("alpha");
        var zulu = CreateBinding("zulu");
        var ascending = manager.Bind(manager.Bind("{}", alpha), zulu);
        var descending = manager.Bind(manager.Bind("{}", zulu), alpha);

        ascending.ShouldBe(descending);
        ascending.IndexOf("alpha", StringComparison.Ordinal)
            .ShouldBeLessThan(ascending.IndexOf("zulu", StringComparison.Ordinal));
    }

    // Verilen setting degerini donduren manager kurar.
    /// <summary>Sahte setting provider uzerine manager kurar.</summary>
    private static RunEnvironmentBindingManager CreateManager(string? configured)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(TestModuleRunSettingNames.EnvironmentBindings).Returns(configured);
        return new RunEnvironmentBindingManager(settings);
    }

    // Yazma testleri icin gecerli bir ortam baglamasi kurar.
    /// <summary>Dogrulanabilir alanlarla dolu yeni baglama modeli dondurur.</summary>
    private static TestRunEnvironmentBinding CreateBinding(string environmentKey = "staging")
    {
        return new TestRunEnvironmentBinding
        {
            EnvironmentKey = environmentKey,
            BaseUrl = "https://staging.example.test",
            SpecSnapshotId = Guid.NewGuid(),
            DbConnectionId = Guid.NewGuid(),
            SecretRef = "vault/staging"
        };
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
