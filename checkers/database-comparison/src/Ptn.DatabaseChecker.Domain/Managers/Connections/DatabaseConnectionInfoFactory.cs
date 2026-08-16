using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Interface.Secrets;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Connections;
using Ptn.DatabaseChecker.Settings;
using Volo.Abp.DependencyInjection;
namespace Ptn.DatabaseChecker.Managers.Connections;
// islevi: Kayitli bir baglanti entity'sini, sifresi Vault'tan cozulmus calisma-zamani baglanti modeline cevirir.
// sistemdeki gorevi: "secret coz + adres birlestir" akisi hem baglanti test'i hem sema kesfi tarafindan paylasilir; tek kaynak burasidir (do-it-once). Sifre yalniz bellege cozulur, DTO'ya/log'a gitmez.
public class DatabaseConnectionInfoFactory : ITransientDependency
{
    private readonly ISecretProvider _secretProvider;
    private readonly ConnectionSafetyProfileResolver? _safetyProfileResolver;
    // islevi: 0.1.x constructor imzasini varsayilan emniyet profiliyle ikili uyumlu tutar.
    public DatabaseConnectionInfoFactory(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    // islevi: Factory'yi secret cozumleyici ve tek emniyet profili resolver'i ile kurar.
    public DatabaseConnectionInfoFactory(
        ISecretProvider secretProvider,
        ConnectionSafetyProfileResolver safetyProfileResolver)
    {
        _secretProvider = secretProvider;
        _safetyProfileResolver = safetyProfileResolver;
    }
    // islevi: Entity adres bilgisi ile Vault kimligini birlestirip provider'larin runtime baglanti modelini kurar.
    public Task<DatabaseConnectionInfo> BuildAsync(DatabaseConnection connection)
        => BuildAsync(connection, default);

    public async Task<DatabaseConnectionInfo> BuildAsync(
        DatabaseConnection connection,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safetyProfile = _safetyProfileResolver is null
            ? CreateDefaultSafetyProfile(connection)
            : await _safetyProfileResolver.ResolveAsync(connection);
        cancellationToken.ThrowIfCancellationRequested();
        var credential = await _secretProvider.GetDatabaseCredentialAsync(connection.VaultSecretPath);
        cancellationToken.ThrowIfCancellationRequested();
        return new DatabaseConnectionInfo
        {
            Host = connection.Host,
            Port = connection.Port,
            DatabaseName = connection.DatabaseName,
            Username = credential.Username,
            Password = credential.Password,
            SafetyProfile = safetyProfile
        };
    }

    // islevi: Eski constructor ile olusan tuketicilere yeni zorunlu runtime profilinin guvenli varsayilanlarini verir.
    private static ConnectionSafetyProfile CreateDefaultSafetyProfile(DatabaseConnection connection)
        => new(
            DatabaseCheckerSettings.Connection.DefaultConnectTimeoutSeconds,
            DatabaseCheckerSettings.Connection.DefaultStatementTimeoutSeconds,
            DatabaseCheckerSettings.Connection.DefaultLockTimeoutSeconds,
            DatabaseCheckerSettings.Connection.DefaultReadOnlyTransaction,
            DatabaseCheckerSettings.Connection.DefaultApplicationNamePrefix,
            connection.TlsModeCode,
            connection.TrustServerCertificate);
}
