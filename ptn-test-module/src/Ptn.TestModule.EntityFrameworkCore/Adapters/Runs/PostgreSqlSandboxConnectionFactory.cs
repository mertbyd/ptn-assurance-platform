using System.Data.Common;
using Npgsql;
using Ptn.TestModule.Interface.Runs;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.EntityFrameworkCore.Adapters.Runs;

// islevi: Ayri sandbox connection-string'ini PostgreSQL provider baglantisina cevirir.
// sistemdeki gorevi: Npgsql sahipligini provider katmaninda tutup Application reset akisina DbConnection verir.
/// <summary>Sandbox reset icin PostgreSQL baglantisi olusturan provider adapter'idir.</summary>
public sealed class PostgreSqlSandboxConnectionFactory
    : ITestDataSandboxConnectionFactory, ITransientDependency
{
    /// <inheritdoc />
    public DbConnection Create(string connectionString) => new NpgsqlConnection(connectionString);
}
