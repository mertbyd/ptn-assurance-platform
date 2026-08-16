using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.SqlServer;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Connections;

// islevi: SQL Server kimliginin sysadmin, db_owner ve db_datawriter rollerini salt-okuma sorgusuyla olcer.
// sistemdeki gorevi: Baglanti testi icin yazma/superuser bulgusunu motor SQL'i sizdirmadan ortak sonuc modeline indirger.
public class SqlServerEnginePrivilegeProbe : IEnginePrivilegeProbe, ITransientDependency
{
    public string EngineCode => DatabaseEngineCodes.SqlServer;

    // islevi: SQL Server rol uyeliklerini tek round-trip ile okur.
    public async Task<EnginePrivilegeProbeResult> ProbeAsync(
        DbConnection connection,
        CancellationToken cancellationToken = default)
    {
        await using var context = SqlServerCatalogDbContext.Create(connection);
        var flags = await context.Set<SqlServerSchemaCatalogRow>()
            .Select(_ => new
            {
                IsSysAdmin = (SqlServerCatalogDbContext.IsServerRoleMember("sysadmin") ?? 0) == 1,
                IsDatabaseOwner = (SqlServerCatalogDbContext.IsRoleMember("db_owner") ?? 0) == 1,
                IsDataWriter = (SqlServerCatalogDbContext.IsRoleMember("db_datawriter") ?? 0) == 1
            })
            .FirstAsync(cancellationToken);
        return CreateResult(flags.IsSysAdmin, flags.IsDatabaseOwner, flags.IsDataWriter);
    }

    // islevi: SQL Server probe bayraklarini provider-bagimsiz fazla yetki sonucuna cevirir.
    internal static EnginePrivilegeProbeResult CreateResult(
        bool isSysAdmin,
        bool isDatabaseOwner,
        bool isDataWriter)
    {
        var canWrite = isSysAdmin || isDatabaseOwner || isDataWriter;
        return new EnginePrivilegeProbeResult
        {
            CanWrite = canWrite,
            IsSuperUser = isSysAdmin,
            WarningCode = canWrite ? DatabaseConnectionExceptionCodes.ExcessivePrivilege : null
        };
    }
}
