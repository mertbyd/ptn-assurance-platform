using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Respawn;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Runs;

// islevi: Ortama ozel yazma yetkili PostgreSQL baglantisini acip dogrulanmis Respawn planini uygular.
// sistemdeki gorevi: Checker hedef baglantisini hic gormeden SUT test verisini kosumdan once bilinen bos duruma getirir.
public sealed class TestDataSandboxService : ITestDataSandbox, ITransientDependency
{
    /// <summary>Reset stratejisi ile ayri baglanti adinin domain sahibidir.</summary>
    private readonly SandboxResetPlanner _planner;

    /// <summary>Tenant-aware adlandirilmis connection string'leri cozen ABP siniridir.</summary>
    private readonly IConnectionStringResolver _connectionStringResolver;
    private readonly ITestDataSandboxConnectionFactory _connectionFactory;

    // I/O sinirini planner ve ABP connection-string resolver ile kurar.
    /// <summary>Sandbox servisini dogrulanmis plan ve ayri baglanti cozumleyicisiyle kurar.</summary>
    public TestDataSandboxService(
        SandboxResetPlanner planner,
        IConnectionStringResolver connectionStringResolver,
        ITestDataSandboxConnectionFactory connectionFactory)
    {
        _planner = planner;
        _connectionStringResolver = connectionStringResolver;
        _connectionFactory = connectionFactory;
    }

    // Dogrulanmis ayri baglantiyi acar ve transaction rollback kullanmadan veriyi sifirlar.
    /// <summary>Verilen ortam icin PostgreSQL sandbox verisini Respawn ile temizler.</summary>
    public async Task ResetAsync(
        string environmentKey,
        CancellationToken cancellationToken = default)
    {
        var plan = await _planner.CreatePlanAsync(environmentKey, cancellationToken);
        var connectionString = await _connectionStringResolver.ResolveAsync(plan.ConnectionStringName);
        await using var connection = _connectionFactory.Create(connectionString);
        await connection.OpenAsync(cancellationToken);
        var respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres });
        await respawner.ResetAsync(connection);
    }
}
