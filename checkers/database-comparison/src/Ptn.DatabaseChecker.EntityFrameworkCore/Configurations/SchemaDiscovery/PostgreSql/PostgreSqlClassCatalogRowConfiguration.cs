using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_class katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class PostgreSqlClassCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlClassCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlClassCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgClassTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.OidColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.RelNameColumn);
        builder.Property(row => row.Kind).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.RelKindColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
        builder.Property(row => row.NamespaceId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.RelNamespaceColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
    }
}
