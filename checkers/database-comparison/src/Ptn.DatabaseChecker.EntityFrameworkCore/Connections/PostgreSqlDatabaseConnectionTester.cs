using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Connections;

// islevi: PostgreSQL hedeflerine Npgsql ile baglanip erisilebilirligi ve sunucu surumunu dogrular.
// sistemdeki gorevi: IDatabaseConnectionTester'in PostgreSql implementasyonu; EngineCode ile secilir, yalniz EFCore katmanindadir. Baglanti hatasi test'in normal sonucudur; bu yuzden surucu istisnasi (DbException) yakalanip ConnectionTestResult'a cevrilir - defensive-noise degil, "test" operasyonunun sozlesmesi.
public class PostgreSqlDatabaseConnectionTester : IDatabaseConnectionTester, ITransientDependency
{
    private readonly IEngineComponentResolver<IEnginePrivilegeProbe>? _privilegeProbeResolver;

    // islevi: 0.1.x tuketicileri icin privilege-probe oncesi constructor imzasini korur.
    public PostgreSqlDatabaseConnectionTester()
    {
    }

    // islevi: Tester'i motor-ozel privilege probe resolver'i ile kurar.
    public PostgreSqlDatabaseConnectionTester(
        IEngineComponentResolver<IEnginePrivilegeProbe> privilegeProbeResolver)
    {
        _privilegeProbeResolver = privilegeProbeResolver;
    }

    public string EngineCode => DatabaseEngineCodes.PostgreSql;

    // islevi: PostgreSQL erisimi ve en az yetki bulgusunu tek acik baglanti uzerinden raporlar.
    public async Task<ConnectionTestResult> TestAsync(DatabaseConnectionInfo info, CancellationToken cancellationToken = default)
    {
        var connectionString = DatabaseConnectionStringFactory.BuildPostgreSql(info);

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            var privileges = _privilegeProbeResolver is null
                ? new EnginePrivilegeProbeResult()
                : await _privilegeProbeResolver.Resolve(EngineCode).ProbeAsync(connection, cancellationToken);
            return new ConnectionTestResult
            {
                Succeeded = true,
                ServerVersion = connection.PostgreSqlVersion.ToString(),
                CanWrite = privileges.CanWrite,
                IsSuperUser = privileges.IsSuperUser,
                PrivilegeWarningCode = privileges.WarningCode
            };
        }
        catch (DbException exception)
        {
            // Baglanti kurulamamasi test'in beklenen sonucudur; surucu istisnasini basarisiz sonuca ceviririz. Program hatalari (ArgumentException vb.) propagate eder.
            return new ConnectionTestResult { Succeeded = false, Message = exception.Message };
        }
    }
}
