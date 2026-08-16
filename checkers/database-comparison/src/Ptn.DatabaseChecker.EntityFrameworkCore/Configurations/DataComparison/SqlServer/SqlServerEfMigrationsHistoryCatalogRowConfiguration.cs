using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Models.Comparison;
using EfHistory = Ptn.DatabaseChecker.Constants.Comparison.DatabaseDataComparisonConstants.EntityFrameworkMigrationsHistory;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.DataComparison.SqlServer;

// islevi: __EFMigrationsHistory katalog satirinin SQL Server katalog okuma context'indeki dbo.__EFMigrationsHistory eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline/migration'a girmez; T7 migration okumasi ham SQL yerine LINQ ile bu tablo uzerinden yapilir.
internal sealed class SqlServerEfMigrationsHistoryCatalogRowConfiguration
    : IEntityTypeConfiguration<EfMigrationsHistoryCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<EfMigrationsHistoryCatalogRow> builder)
    {
        builder.ToTable(EfHistory.TableName, EfHistory.SqlServerDefaultSchema);
        builder.HasKey(row => row.MigrationId);
        builder.Property(row => row.MigrationId).HasColumnName(EfHistory.MigrationIdColumn);
        builder.Property(row => row.ProductVersion).HasColumnName(EfHistory.ProductVersionColumn);
    }
}
