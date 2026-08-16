using System.Text;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Managers.Bridge.Profiles;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Bridge.Profiles;

// islevi: Is kurali belgesinin muhur ve butce kararlarini dogrular.
// sistemdeki gorevi: rules_fingerprint'in profil paketiyle ayni sozlesmeyi tasidigini ve butce disi kaynagi reddettigini garanti eder.
public class BusinessRuleFingerprintManagerTests
{
    private static readonly BusinessRuleFingerprintManager Manager = new();

    // Muhur profil paketiyle ayni lowercase sha256: sozlesmesini tasimalidir.
    [Fact]
    public void Should_seal_content_with_the_shared_fingerprint_contract()
    {
        var fingerprint = Manager.ComputeFingerprint(Encoding.UTF8.GetBytes("# kural"));

        fingerprint.ShouldStartWith(PtnBridgeSettingNames.FingerprintPrefix);
        fingerprint.ShouldBe(fingerprint.ToLowerInvariant());
        fingerprint.Length.ShouldBe(PtnBridgeSettingNames.FingerprintPrefix.Length + 64);
    }

    // Ayni icerik her cagride ayni muhuru uretmelidir; aksi halde malzeme kaymasi yanlis alarm verir.
    [Fact]
    public void Should_stay_stable_for_the_same_content()
    {
        var bytes = Encoding.UTF8.GetBytes("# kural");

        Manager.ComputeFingerprint(bytes).ShouldBe(Manager.ComputeFingerprint(bytes));
    }

    // Degisen kural metni muhuru degistirmelidir; aksi halde kayma hic yakalanmaz.
    [Fact]
    public void Should_change_when_the_rule_content_changes()
    {
        var first = Manager.ComputeFingerprint(Encoding.UTF8.GetBytes("# kural"));
        var second = Manager.ComputeFingerprint(Encoding.UTF8.GetBytes("# baska kural"));

        first.ShouldNotBe(second);
    }

    // Bos kaynak muhurlenemez; yayin kapisi bos malzemeyle acilmamalidir.
    [Fact]
    public void Should_reject_an_empty_rule_source()
    {
        var exception = Should.Throw<BusinessException>(() => Manager.EnsureWithinBudget(0));

        exception.Code.ShouldBe(TestModuleBridgeErrorCodes.BusinessRulesInvalid);
    }

    // Butce disi kaynak okunmadan reddedilmelidir.
    [Fact]
    public void Should_reject_a_rule_source_above_the_budget()
    {
        var exception = Should.Throw<BusinessException>(() =>
            Manager.EnsureWithinBudget(PtnBridgeConsts.MaxBusinessRulesBytes + 1));

        exception.Code.ShouldBe(TestModuleBridgeErrorCodes.BusinessRulesInvalid);
    }
}
