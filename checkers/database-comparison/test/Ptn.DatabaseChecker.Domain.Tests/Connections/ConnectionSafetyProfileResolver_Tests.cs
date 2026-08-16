using System;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.DatabaseChecker.Connections;

// islevi: ConnectionSafetyProfileResolver'in entity TLS kararlariyla ABP setting zincirini birlestirmesini dogrular.
// sistemdeki gorevi: Guvenli TrustServerCertificate varsayilanini ve timeout/application-name ayarlarinin tek cozum noktasini korur.
public class ConnectionSafetyProfileResolver_Tests
{
    [Fact]
    public async Task Entity_Tls_Fields_Should_Win_While_Timeouts_Come_From_Settings()
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.Connection.ConnectTimeoutSeconds).Returns("12");
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.Connection.StatementTimeoutSeconds).Returns("34");
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.Connection.LockTimeoutSeconds).Returns("6");
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.Connection.ReadOnlyTransaction).Returns("false");
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.Connection.ApplicationNamePrefix).Returns("Custom.Checker");
        var resolver = new ConnectionSafetyProfileResolver(settingProvider);
        var connection = new DatabaseConnection(Guid.NewGuid())
        {
            TlsModeCode = TlsModeCodes.Prefer,
            TrustServerCertificate = true
        };

        var profile = await resolver.ResolveAsync(connection);

        profile.ConnectTimeoutSeconds.ShouldBe(12);
        profile.StatementTimeoutSeconds.ShouldBe(34);
        profile.LockTimeoutSeconds.ShouldBe(6);
        profile.ReadOnlyTransaction.ShouldBeFalse();
        profile.ApplicationName.ShouldStartWith("Custom.Checker/");
        profile.TlsModeCode.ShouldBe(TlsModeCodes.Prefer);
        profile.TrustServerCertificate.ShouldBeTrue();
    }

    [Fact]
    public async Task TrustServerCertificate_Should_Default_To_False()
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        var resolver = new ConnectionSafetyProfileResolver(settingProvider);
        var connection = new DatabaseConnection(Guid.NewGuid());
        var profile = await resolver.ResolveAsync(connection);
        profile.TlsModeCode.ShouldBe(TlsModeCodes.Require);
        profile.TrustServerCertificate.ShouldBeFalse();
        profile.ConnectTimeoutSeconds.ShouldBe(DatabaseCheckerSettings.Connection.DefaultConnectTimeoutSeconds);
        profile.StatementTimeoutSeconds.ShouldBe(DatabaseCheckerSettings.Connection.DefaultStatementTimeoutSeconds);
        profile.LockTimeoutSeconds.ShouldBe(DatabaseCheckerSettings.Connection.DefaultLockTimeoutSeconds);
        profile.ReadOnlyTransaction.ShouldBeTrue();
    }

    [Fact]
    public async Task Invalid_Tls_Mode_Should_Fail_With_Stable_Business_Code()
    {
        var resolver = new ConnectionSafetyProfileResolver(Substitute.For<ISettingProvider>());
        var connection = new DatabaseConnection(Guid.NewGuid()) { TlsModeCode = "invalid" };

        var exception = await Should.ThrowAsync<BusinessException>(() => resolver.ResolveAsync(connection));

        exception.Code.ShouldBe(DatabaseConnectionExceptionCodes.InvalidTlsMode);
    }
}
