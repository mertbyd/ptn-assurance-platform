using Microsoft.EntityFrameworkCore;
using Ptn.DatabaseChecker.EntityFrameworkCore.Configurations;
using Volo.Abp;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

public static class DatabaseCheckerDbContextModelCreatingExtensions
{
    public static void ConfigureDatabaseChecker(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // IEntityTypeConfiguration<> dosyalarini (Configurations/ klasoru) otomatik tarar ve uygular.
        // Migration DbContext bu metodu cagirdigi icin entity'ler migration'a dahil olur.
        // External DB katalog okuma context'lerine ait ICatalogModelConfiguration'lar haric tutulur; onlar app modeline/migration'a girmez, kendi katalog DbContext'lerinde guvenli namespace taramasiyla uygulanir.
        builder.ApplyConfigurationsFromAssembly(
            typeof(DatabaseCheckerDbContextModelCreatingExtensions).Assembly,
            type => !typeof(ICatalogModelConfiguration).IsAssignableFrom(type));
    }
}
