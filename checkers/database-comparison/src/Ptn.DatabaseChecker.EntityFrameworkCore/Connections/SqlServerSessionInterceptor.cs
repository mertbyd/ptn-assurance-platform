using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ptn.DatabaseChecker.Models.Connections;

namespace Ptn.DatabaseChecker.Connections;

// islevi: SQL Server katalog DbContext baglantisi acildiginda profil session komutlarini uygular.
// sistemdeki gorevi: LOCK_TIMEOUT baglanti dizesinde desteklenmedigi icin kurulumun tek kaynagi olan SqlServerCatalogDbContext.Create akisina baglanir.
internal sealed class SqlServerSessionInterceptor : DbConnectionInterceptor
{
    private readonly ConnectionSafetyProfile _profile;

    // islevi: Interceptor'u baglantiya uygulanacak cozulmus emniyet profiliyle kurar.
    public SqlServerSessionInterceptor(ConnectionSafetyProfile profile)
    {
        _profile = profile;
    }

    // islevi: Senkron acilan katalog baglantisinda session politikasini uygular.
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => SqlServerSessionInitializer.Apply(connection, _profile);

    // islevi: Asenkron acilan katalog baglantisinda session politikasini uygular.
    public override Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => SqlServerSessionInitializer.ApplyAsync(connection, _profile, cancellationToken);
}
