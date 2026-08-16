using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Models.Connections;

namespace Ptn.DatabaseChecker.Connections;

// islevi: SQL Server acik baglantisina profil kaynakli session komutlarini tek noktadan uygular.
// sistemdeki gorevi: Katalog interceptor'u ile connection tester ayni LOCK_TIMEOUT komut metnini ve timeout davranisini paylasir.
internal static class SqlServerSessionInitializer
{
    private const int MillisecondsPerSecond = 1000;

    // islevi: Senkron acilan EF baglantisina SQL Server session politikasini bir kez uygular.
    internal static void Apply(DbConnection connection, ConnectionSafetyProfile profile)
    {
        using var command = CreateCommand(connection, profile);
        command.ExecuteNonQuery();
    }

    // islevi: Asenkron acilan EF/test baglantisina SQL Server session politikasini bir kez uygular.
    internal static async Task ApplyAsync(
        DbConnection connection,
        ConnectionSafetyProfile profile,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(connection, profile);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // islevi: Profildeki saniye degerini SQL Server'in milisaniyelik LOCK_TIMEOUT session komutuna cevirir.
    internal static string BuildCommandText(ConnectionSafetyProfile profile)
        => FormattableString.Invariant($"SET LOCK_TIMEOUT {checked(profile.LockTimeoutSeconds * MillisecondsPerSecond)};");

    // islevi: Session komutunu profil statement timeout'u ile calisacak sekilde hazirlar.
    private static DbCommand CreateCommand(DbConnection connection, ConnectionSafetyProfile profile)
    {
        var command = connection.CreateCommand();
        command.CommandText = BuildCommandText(profile);
        command.CommandTimeout = profile.StatementTimeoutSeconds;
        return command;
    }
}
