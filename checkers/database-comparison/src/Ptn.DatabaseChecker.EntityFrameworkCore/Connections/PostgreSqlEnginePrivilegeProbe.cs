using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.PostgreSql;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Connections;

// islevi: PostgreSQL kimliginin pg_write_all_data, superuser ve database CREATE yetkilerini salt-okuma sorgusuyla olcer.
// sistemdeki gorevi: Baglanti testini bozmayacak bir en az yetki bulgusu uretir; fazla yetki basarili sonuca uyari kodu olarak eklenir.
public class PostgreSqlEnginePrivilegeProbe : IEnginePrivilegeProbe, ITransientDependency
{
    public string EngineCode => DatabaseEngineCodes.PostgreSql;

    // islevi: PostgreSQL rol ve database yetkilerini tek round-trip ile okur.
    public async Task<EnginePrivilegeProbeResult> ProbeAsync(
        DbConnection connection,
        CancellationToken cancellationToken = default)
    {
        var userName = new NpgsqlConnectionStringBuilder(connection.ConnectionString).Username ?? string.Empty;
        await using var context = PostgreSqlCatalogDbContext.Create(connection);
        var flags = await context.Set<PostgreSqlNamespaceCatalogRow>()
            .Select(_ => new
            {
                HasWriteRole = PostgreSqlCatalogDbContext.HasRole(userName, "pg_write_all_data", "member"),
                IsSuperUser = PostgreSqlCatalogDbContext.CurrentSetting("is_superuser") == "on",
                CanCreate = PostgreSqlCatalogDbContext.HasDatabasePrivilege(
                    userName,
                    connection.Database,
                    "CREATE")
            })
            .FirstAsync(cancellationToken);
        return CreateResult(flags.HasWriteRole, flags.IsSuperUser, flags.CanCreate);
    }

    // islevi: PostgreSQL probe bayraklarini provider-bagimsiz fazla yetki sonucuna cevirir.
    internal static EnginePrivilegeProbeResult CreateResult(
        bool hasWriteAllDataRole,
        bool isSuperUser,
        bool canCreateInDatabase)
    {
        var canWrite = hasWriteAllDataRole || isSuperUser || canCreateInDatabase;
        return new EnginePrivilegeProbeResult
        {
            CanWrite = canWrite,
            IsSuperUser = isSuperUser,
            WarningCode = canWrite ? DatabaseConnectionExceptionCodes.ExcessivePrivilege : null
        };
    }
}
