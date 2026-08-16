using System;
using Microsoft.Data.SqlClient;
using Npgsql;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Connections;
using Volo.Abp;

namespace Ptn.DatabaseChecker.Connections;

// islevi: Provider bazli connection string uretimini emniyet profiliyle tek noktada toplar.
// sistemdeki gorevi: Tester ve katalog context'leri timeout, application-name, TLS ve PostgreSQL session GUC kararlarini kopyalamaz.
internal static class DatabaseConnectionStringFactory
{
    // islevi: PostgreSQL icin profil-surumlu Npgsql connection string'i uretir.
    internal static string BuildPostgreSql(DatabaseConnectionInfo info)
        => new NpgsqlConnectionStringBuilder
        {
            Host = info.Host,
            Port = info.Port,
            Database = info.DatabaseName,
            Username = info.Username,
            Password = info.Password,
            Timeout = info.SafetyProfile.ConnectTimeoutSeconds,
            CommandTimeout = info.SafetyProfile.StatementTimeoutSeconds,
            ApplicationName = info.SafetyProfile.ApplicationName,
            SslMode = ResolvePostgreSqlSslMode(info.SafetyProfile.TlsModeCode),
            Options = BuildPostgreSqlOptions(info.SafetyProfile)
        }.ConnectionString;

    // islevi: SQL Server icin profil-surumlu SqlClient connection string'i uretir.
    internal static string BuildSqlServer(DatabaseConnectionInfo info)
        => new SqlConnectionStringBuilder
        {
            DataSource = $"{info.Host},{info.Port}",
            InitialCatalog = info.DatabaseName,
            UserID = info.Username,
            Password = info.Password,
            ConnectTimeout = info.SafetyProfile.ConnectTimeoutSeconds,
            CommandTimeout = info.SafetyProfile.StatementTimeoutSeconds,
            ApplicationName = info.SafetyProfile.ApplicationName,
            Encrypt = ResolveSqlServerEncryption(info.SafetyProfile.TlsModeCode),
            TrustServerCertificate = info.SafetyProfile.TrustServerCertificate
        }.ConnectionString;

    // islevi: PostgreSQL statement/lock timeout ve read-only varsayilanini startup Options GUC dizisine cevirir.
    private static string BuildPostgreSqlOptions(ConnectionSafetyProfile profile)
        => FormattableString.Invariant($"-c statement_timeout={profile.StatementTimeoutSeconds}s -c lock_timeout={profile.LockTimeoutSeconds}s -c default_transaction_read_only={(profile.ReadOnlyTransaction ? "on" : "off")}");

    // islevi: Kararli TLS kodunu Npgsql SSL moduna cevirir.
    private static SslMode ResolvePostgreSqlSslMode(string tlsModeCode)
        => tlsModeCode switch
        {
            TlsModeCodes.Require => SslMode.Require,
            TlsModeCodes.Prefer => SslMode.Prefer,
            TlsModeCodes.Disable => SslMode.Disable,
            _ => throw new BusinessException(DatabaseConnectionExceptionCodes.InvalidTlsMode)
        };

    // islevi: Kararli TLS kodunu SqlClient sifreleme politikasina cevirir; Optional sunucu zorunluluguna izin verir, Require istemci tarafinda TLS'i zorlar.
    private static SqlConnectionEncryptOption ResolveSqlServerEncryption(string tlsModeCode)
        => tlsModeCode switch
        {
            TlsModeCodes.Require => SqlConnectionEncryptOption.Mandatory,
            TlsModeCodes.Prefer => SqlConnectionEncryptOption.Optional,
            TlsModeCodes.Disable => SqlConnectionEncryptOption.Optional,
            _ => throw new BusinessException(DatabaseConnectionExceptionCodes.InvalidTlsMode)
        };
}
