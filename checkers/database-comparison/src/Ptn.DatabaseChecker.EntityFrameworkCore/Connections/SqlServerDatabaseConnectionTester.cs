using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Connections;

// islevi: SQL Server hedeflerine Microsoft.Data.SqlClient ile baglanip erisilebilirligi ve sunucu surumunu dogrular.
// sistemdeki gorevi: IDatabaseConnectionTester'in SqlServer implementasyonu; EngineCode ile secilir, yalniz EFCore katmanindadir. Baglanti hatasi normal sonuctur; surucu istisnasi (DbException) ConnectionTestResult'a cevrilir.
public class SqlServerDatabaseConnectionTester : IDatabaseConnectionTester, ITransientDependency
{
    private readonly IEngineComponentResolver<IEnginePrivilegeProbe>? _privilegeProbeResolver;

    // islevi: 0.1.x tuketicileri icin privilege-probe oncesi constructor imzasini korur.
    public SqlServerDatabaseConnectionTester()
    {
    }

    // islevi: Tester'i motor-ozel privilege probe resolver'i ile kurar.
    public SqlServerDatabaseConnectionTester(
        IEngineComponentResolver<IEnginePrivilegeProbe> privilegeProbeResolver)
    {
        _privilegeProbeResolver = privilegeProbeResolver;
    }

    public string EngineCode => DatabaseEngineCodes.SqlServer;

    // islevi: SQL Server erisimi, session politikasini ve en az yetki bulgusunu tek acik baglanti uzerinden raporlar.
    public async Task<ConnectionTestResult> TestAsync(DatabaseConnectionInfo info, CancellationToken cancellationToken = default)
    {
        var connectionString = DatabaseConnectionStringFactory.BuildSqlServer(info);

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await SqlServerSessionInitializer.ApplyAsync(connection, info.SafetyProfile, cancellationToken);
            var privileges = _privilegeProbeResolver is null
                ? new EnginePrivilegeProbeResult()
                : await _privilegeProbeResolver.Resolve(EngineCode).ProbeAsync(connection, cancellationToken);
            return new ConnectionTestResult
            {
                Succeeded = true,
                ServerVersion = connection.ServerVersion,
                CanWrite = privileges.CanWrite,
                IsSuperUser = privileges.IsSuperUser,
                PrivilegeWarningCode = privileges.WarningCode
            };
        }
        catch (DbException exception)
        {
            // Baglanti kurulamamasi test'in beklenen sonucudur; surucu istisnasini basarisiz sonuca ceviririz.
            return new ConnectionTestResult { Succeeded = false, Message = exception.Message };
        }
    }
}
