using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.Connections;
using Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.DataComparison.SqlServer;
using Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;
using Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.SchemaDiscovery.SqlServer;

// islevi: SQL Server sys kataloglarini migration disi okuyan EF Core context'idir.
// sistemdeki gorevi: Uygulamanin ana metadata DbContext'ine sys mapping'i karistirmadan external DB kataloglarini LINQ ile sorgulatir; SQL Server katalog konfigurasyonlarini guvenli namespace taramasiyla alir. Entity'ler ICatalogModelConfiguration implementasyonlari uzerinden otomatik kesfedilir; yeni katalog tablosu eklemek icin sadece CatalogRow + Configuration yeterlir, context'e DbSet eklenmez.
internal sealed class SqlServerCatalogDbContext : DbContext
{
    public SqlServerCatalogDbContext(DbContextOptions<SqlServerCatalogDbContext> options)
        : base(options)
    {
    }

    // islevi: Hedef SQL Server baglantisina baglanan migration-disi katalog context'ini kurar.
    // sistemdeki gorevi: Okuyucu repository kurulum kodunu kopyalamaz; kurulum tek kaynak olarak burada yasar (do-it-once).
    public static SqlServerCatalogDbContext Create(DatabaseConnectionInfo info)
    {
        var options = new DbContextOptionsBuilder<SqlServerCatalogDbContext>()
            .UseSqlServer(DatabaseConnectionStringFactory.BuildSqlServer(info))
            .AddInterceptors(new SqlServerSessionInterceptor(info.SafetyProfile))
            .Options;

        return new SqlServerCatalogDbContext(options);
    }

    // islevi: Tester'in acik SQL Server baglantisini kapatmadan LINQ privilege probe context'ine baglar.
    public static SqlServerCatalogDbContext Create(DbConnection connection)
    {
        var options = new DbContextOptionsBuilder<SqlServerCatalogDbContext>()
            .UseSqlServer(connection, contextOwnsConnection: false)
            .Options;
        return new SqlServerCatalogDbContext(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Sema kesfi (sys) katalog mapping'leri.
        modelBuilder.ApplyCatalogConfigurationsFromNamespace(typeof(SqlServerSchemaCatalogRowConfiguration));
        // T7 veri/migration karsilastirmasi: dbo.__EFMigrationsHistory mapping'i (ayni context, ham SQL yerine LINQ).
        modelBuilder.ApplyCatalogConfigurationsFromNamespace(typeof(SqlServerEfMigrationsHistoryCatalogRowConfiguration));
    }

    // islevi: SQL Server nesne tanimini (trigger/procedure vb.) ham SQL metni olarak dondurur.
    [DbFunction("OBJECT_DEFINITION", IsBuiltIn = true)]
    public static string? ObjectDefinition(int objectId)
        => throw new NotSupportedException();

    // islevi: SQL Server server-role uyeligini scalar motor fonksiyonuyla sorgular.
    [DbFunction("IS_SRVROLEMEMBER", IsBuiltIn = true)]
    public static int? IsServerRoleMember(string roleName)
        => throw new NotSupportedException();

    // islevi: SQL Server database-role uyeligini scalar motor fonksiyonuyla sorgular.
    [DbFunction("IS_ROLEMEMBER", IsBuiltIn = true)]
    public static int? IsRoleMember(string roleName)
        => throw new NotSupportedException();
}
