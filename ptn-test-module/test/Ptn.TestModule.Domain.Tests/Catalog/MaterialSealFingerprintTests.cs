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
}
