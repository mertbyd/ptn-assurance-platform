using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.EntityFrameworkCore.Configurations;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_settings katalog satirinin external okuma context'indeki tablo ve kolon eslemesini tanimlar.
// sistemdeki gorevi: ICatalogModelConfiguration taramasiyla yalniz PostgreSqlCatalogDbContext'e girer; ana modele veya migration'a eklenmez.
internal sealed class PostgreSqlSettingCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlSettingCatalogRow>, ICatalogModelConfiguration
{
    // islevi: Setting adi anahtarini ve etkin setting degerini PostgreSQL katalog kolonlarina esler.
    public void Configure(EntityTypeBuilder<PostgreSqlSettingCatalogRow> builder)
    {
        builder.ToTable(
            DatabaseMetadataCatalogConstants.PostgreSql.PgSettingsTable,
            DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.Name);
        builder.Property(row => row.Name)
            .HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SettingNameColumn);
        builder.Property(row => row.Setting)
            .HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SettingValueColumn);
    }
}
