using System.Reflection;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Connections;
using Ptn.DatabaseChecker.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Ptn.DatabaseChecker.Managers.Connections;

// islevi: Baglanti entity'si ile tenant -> global -> default ayar zincirini tek bir emniyet profiline cozer.
// sistemdeki gorevi: TLS karari entity'den, timeout/read-only/uygulama kimligi ayarlardan gelir; profil baska hicbir yerde yeniden kurulmaz.
public class ConnectionSafetyProfileResolver : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    // islevi: Resolver'i ABP'nin tenant-aware setting okuyucusuyla kurar.
    public ConnectionSafetyProfileResolver(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    // islevi: Kayitli baglanti icin provider'larin kullanacagi tek runtime emniyet profilini uretir.
    public async Task<ConnectionSafetyProfile> ResolveAsync(DatabaseConnection connection)
    {
        if (!TlsModeCodes.IsDefined(connection.TlsModeCode))
        {
            throw new BusinessException(DatabaseConnectionExceptionCodes.InvalidTlsMode);
        }

        var connectTimeoutSeconds = await _settingProvider.GetAsync(
            DatabaseCheckerSettings.Connection.ConnectTimeoutSeconds,
            DatabaseCheckerSettings.Connection.DefaultConnectTimeoutSeconds);
        var statementTimeoutSeconds = await _settingProvider.GetAsync(
            DatabaseCheckerSettings.Connection.StatementTimeoutSeconds,
            DatabaseCheckerSettings.Connection.DefaultStatementTimeoutSeconds);
        var lockTimeoutSeconds = await _settingProvider.GetAsync(
            DatabaseCheckerSettings.Connection.LockTimeoutSeconds,
            DatabaseCheckerSettings.Connection.DefaultLockTimeoutSeconds);
        var readOnlyTransaction = await _settingProvider.GetAsync(
            DatabaseCheckerSettings.Connection.ReadOnlyTransaction,
            DatabaseCheckerSettings.Connection.DefaultReadOnlyTransaction);
        var applicationNamePrefix = await _settingProvider.GetOrNullAsync(
                                        DatabaseCheckerSettings.Connection.ApplicationNamePrefix)
                                    ?? DatabaseCheckerSettings.Connection.DefaultApplicationNamePrefix;

        return new ConnectionSafetyProfile(
            connectTimeoutSeconds,
            statementTimeoutSeconds,
            lockTimeoutSeconds,
            readOnlyTransaction,
            $"{applicationNamePrefix}/{ResolveModuleVersion()}",
            connection.TlsModeCode,
            connection.TrustServerCertificate);
    }

    // islevi: Calisan Domain paketinin informational version bilgisini baglanti application-name kimligine indirger.
    private static string ResolveModuleVersion()
        => typeof(ConnectionSafetyProfileResolver).Assembly
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
               .InformationalVersion.Split('+')[0]
           ?? typeof(ConnectionSafetyProfileResolver).Assembly.GetName().Version!.ToString();
}
