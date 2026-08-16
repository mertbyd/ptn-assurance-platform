using System;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Models.Catalog;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Catalog;

// islevi: Senaryo malzeme muhrunun (material seal) fingerprint kanoniklestirmesini dogrular.
// sistemdeki gorevi: sha256: tel bicimi ile 64-hex veritabani bicimi arasindaki cevirimi guvenceye alir.
public class MaterialSealFingerprintTests
{
    private const string ValidHash = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2";

    public MaterialSealFingerprintTests()
    {
        // Manager'in Normalize mantigi ve StripFingerprintPrefix public/static bagimliliksiz oldugu icin test edilebilir.
    }

    // sha256: prefiksli gecerli hash, prefikssiz 64-hex olarak kabul edilmelidir.
    [Fact]
    public void A_prefixed_fingerprint_should_be_accepted_and_stripped()
    {
        var result = TestScenarioManager.StripFingerprintPrefix("sha256:" + ValidHash);
        result.ShouldBe(ValidHash);
    }

    // Prefikssiz gecerli hash dogrudan kabul edilmelidir.
    [Fact]
    public void An_unprefixed_fingerprint_should_be_accepted_as_is()
    {
        var result = TestScenarioManager.StripFingerprintPrefix(ValidHash);
        result.ShouldBe(ValidHash);
    }

    // Gecersiz prefiks (or. md5:) tasiyan hash'ler prefiksleriyle kalmali ve (Normalize yolunda) reddedilmelidir.
    [Fact]
    public void An_invalid_prefix_should_not_be_stripped()
    {
        var invalid = "md5:" + ValidHash;
        var result = TestScenarioManager.StripFingerprintPrefix(invalid);
        result.ShouldBe(invalid);
    }

    // Istemci deger tasimadiginda sunucudaki aktif kural muhru prefikssiz olarak muhre yazilmalidir.
    [Fact]
    public void An_empty_rules_fingerprint_should_take_the_active_server_value()
    {
        var seal = new TestScenarioMaterialSeal();

        CreateManager().ApplyRulesFingerprint(seal, "sha256:" + ValidHash);

        seal.RulesFingerprint.ShouldBe(ValidHash);
    }

    // Istemcinin prefiksli veya prefikssiz tasidigi ayni deger kabul edilmeli ve prefikssiz saklanmalidir.
    [Theory]
    [InlineData(ValidHash)]
    [InlineData("sha256:" + ValidHash)]
    public void A_matching_rules_fingerprint_should_be_accepted(string provided)
    {
        var seal = new TestScenarioMaterialSeal { RulesFingerprint = provided };

        CreateManager().ApplyRulesFingerprint(seal, "sha256:" + ValidHash);

        seal.RulesFingerprint.ShouldBe(ValidHash);
    }

    // Bayat veya uydurulmus kural muhru sunucudaki aktif bayta baglanmadigi icin reddedilmelidir.
    [Fact]
    public void A_stale_rules_fingerprint_should_be_rejected()
    {
        var seal = new TestScenarioMaterialSeal { RulesFingerprint = new string('b', 64) };

        var exception = Should.Throw<BusinessException>(
            () => CreateManager().ApplyRulesFingerprint(seal, "sha256:" + ValidHash));

        exception.Code.ShouldBe(TestModuleScenarioErrorCodes.InvalidHash);
    }

    // Muhur kurallari repository'e ugramadigi icin Manager bagimliliksiz kurulabilir.
    private static TestScenarioManager CreateManager()
    {
        return new TestScenarioManager(null!, null!, null!);
    }
}
