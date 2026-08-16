using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: FindingValueRedactor'in HMAC, maskeleme ve tam saklama semantigini dogrular.
// sistemdeki gorevi: Ayni girdi icin determinizmi, farkli girdiler icin ayrik hash'i ve ham deger sizintisi olmayan modlari korur.
public class FindingValueRedactor_Tests
{
    [Fact]
    public void Hashed_Mode_Should_Be_Deterministic_And_Value_Sensitive()
    {
        var redactor = new FindingValueRedactor();
        var policy = new ValueRetentionPolicy(ValueRetentionModeCodes.Hashed, new string('s', 16));

        var first = redactor.Redact("Ada", policy);
        var second = redactor.Redact("Ada", policy);
        var different = redactor.Redact("Grace", policy);

        first.ShouldBe(second);
        first.ShouldNotBe(different);
        first.ShouldNotBeNull();
        first!.ShouldNotContain("Ada");
    }

    [Fact]
    public void None_Mode_Should_Remove_Value_And_Full_Mode_Should_Preserve_It()
    {
        var redactor = new FindingValueRedactor();

        redactor.Redact("Ada", new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty)).ShouldBeNull();
        redactor.Redact("Ada", new ValueRetentionPolicy(ValueRetentionModeCodes.Full, string.Empty)).ShouldBe("Ada");
    }

    [Fact]
    public void Masked_Mode_Should_Be_Deterministic_Without_Preserving_Full_Value()
    {
        var redactor = new FindingValueRedactor();
        var policy = new ValueRetentionPolicy(ValueRetentionModeCodes.Masked, string.Empty);

        var first = redactor.Redact("Ali@example.com", policy);
        var second = redactor.Redact("Ali@example.com", policy);

        first.ShouldBe(second);
        first.ShouldBe("A***@***.com");
        first.ShouldNotBe("Ali@example.com");
    }
}
