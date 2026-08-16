using Ptn.DatabaseChecker.Interface.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: 0.1.x namespace'indeki connection tester API'lerini yeni Connections sahibine yonlendirir.
// sistemdeki gorevi: NuGet ikili uyumlulugunu korur; conventional DI'ya ikinci engine implementasyonlari eklemez.
[DisableConventionalRegistration]
public class SqlServerDatabaseConnectionTester
    : Connections.SqlServerDatabaseConnectionTester
{
    public SqlServerDatabaseConnectionTester()
    {
    }

    public SqlServerDatabaseConnectionTester(
        IEngineComponentResolver<IEnginePrivilegeProbe> privilegeProbeResolver)
        : base(privilegeProbeResolver)
    {
    }
}

// PostgreSQL tester'in 0.1.x tam tip adini yeni implementasyona yonlendirir.
[DisableConventionalRegistration]
public class PostgreSqlDatabaseConnectionTester
    : Connections.PostgreSqlDatabaseConnectionTester
{
    public PostgreSqlDatabaseConnectionTester()
    {
    }

    public PostgreSqlDatabaseConnectionTester(
        IEngineComponentResolver<IEnginePrivilegeProbe> privilegeProbeResolver)
        : base(privilegeProbeResolver)
    {
    }
}
