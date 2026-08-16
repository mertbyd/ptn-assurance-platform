using System.Threading.Tasks;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: ValueRetentionPolicyResolver'in ayar fallback ve Hashed salt zorunlulugunu dogrular.
// sistemdeki gorevi: Varsayilan None politikasinin ve salt yoklugunda fail-closed davranisin regresyon kanitidir.
public class ValueRetentionPolicyResolver_Tests
{
    [Fact]
    public async Task Should_Default_To_None_When_No_Setting_Override_Exists()
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        var resolver = new ValueRetentionPolicyResolver(settingProvider);

        var policy = await resolver.ResolveAsync();

        policy.ModeCode.ShouldBe(ValueRetentionModeCodes.None);
        policy.Salt.ShouldBeEmpty();
    }

    [Fact]
    public async Task Hashed_Mode_Should_Reject_Missing_Salt()
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.DataComparison.ValueRetentionMode)
            .Returns(ValueRetentionModeCodes.Hashed);
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.DataComparison.ValueRedactionSalt)
            .Returns(string.Empty);
        var resolver = new ValueRetentionPolicyResolver(settingProvider);

        var exception = await Should.ThrowAsync<BusinessException>(() => resolver.ResolveAsync());

        exception.Code.ShouldBe(DataComparisonExceptionCodes.RedactionSaltMissing);
    }
}
