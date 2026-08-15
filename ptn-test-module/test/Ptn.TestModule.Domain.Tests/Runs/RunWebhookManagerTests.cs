using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Webhook ucunun sirsiz acilmadigini, yanlis sirri reddettigini ve teslim kimligini sabitledigini dogrular.
// sistemdeki gorevi: Anonim ucun tek koruma katmaninin negatif yol kapisidir (§2.2, §5.7).
public class RunWebhookManagerTests
{
    private const string ConfiguredSecret = "s3cr3t-value";

    // Sir ayari tanimlanmadan uc kapalidir; herhangi bir deger gonderilse bile istek reddedilir.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("any-value")]
    public async Task An_unconfigured_secret_should_keep_the_endpoint_closed(string? presented)
    {
        var manager = CreateManager(configuredSecret: null);

        await Should.ThrowAsync<AbpAuthorizationException>(
            () => manager.EnsureAuthorizedAsync(presented));
    }

    // Sir tanimliyken eksik veya yanlis deger reddedilmelidir.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong-value")]
    public async Task A_mismatched_secret_should_be_rejected(string? presented)
    {
        var manager = CreateManager(ConfiguredSecret);

        await Should.ThrowAsync<AbpAuthorizationException>(
            () => manager.EnsureAuthorizedAsync(presented));
    }

    // Sir tanimli ve eslesiyorsa istek gecmelidir.
    [Fact]
    public async Task A_matching_secret_should_be_accepted()
    {
        var manager = CreateManager(ConfiguredSecret);

        await Should.NotThrowAsync(() => manager.EnsureAuthorizedAsync(ConfiguredSecret));
    }

    // Bos teslim kimligi kararli kodla reddedilmelidir.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_delivery_id_should_be_rejected(string deliveryId)
    {
        var exception = Should.Throw<BusinessException>(
            () => RunWebhookManager.NormalizeDeliveryId(deliveryId));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.WebhookDeliveryIdInvalid);
    }

    // Kalici siniri asan teslim kimligi kararli kodla reddedilmelidir.
    [Fact]
    public void An_oversized_delivery_id_should_be_rejected()
    {
        var exception = Should.Throw<BusinessException>(
            () => RunWebhookManager.NormalizeDeliveryId(new string('d', 129)));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.WebhookDeliveryIdInvalid);
    }

    // Teslim kimligi tetikleyici referansi olacagi icin bosluklarindan arindirilmalidir.
    [Fact]
    public void A_delivery_id_should_be_trimmed()
    {
        RunWebhookManager.NormalizeDeliveryId("  delivery-1  ").ShouldBe("delivery-1");
    }

    // Sir dogrulamasi disindaki sinirlar bu testlerde kullanilmaz; yalniz ayar okumasi taklit edilir.
    private static RunWebhookManager CreateManager(string? configuredSecret)
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(TestModuleSettings.WebhookSecret).Returns(configuredSecret);
        return new RunWebhookManager(
            settingProvider,
            Substitute.For<ITestScenarioRepository>(),
            Substitute.For<ITestScenarioStateRepository>(),
            null!);
    }
}
