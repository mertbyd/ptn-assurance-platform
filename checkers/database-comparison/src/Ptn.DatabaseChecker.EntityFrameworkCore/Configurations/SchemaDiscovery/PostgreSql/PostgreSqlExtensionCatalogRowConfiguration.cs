using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_extension katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class PostgreSqlExtensionCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlExtensionCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlExtensionCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgExtensionTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.OidColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ExtNameColumn);
        builder.Property(row => row.NamespaceId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ExtNamespaceColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.Version).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ExtVersionColumn);
    }
}
