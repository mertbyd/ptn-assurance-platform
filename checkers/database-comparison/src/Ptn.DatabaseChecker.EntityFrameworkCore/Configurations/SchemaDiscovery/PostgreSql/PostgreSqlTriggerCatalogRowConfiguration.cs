using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_trigger katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class PostgreSqlTriggerCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlTriggerCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlTriggerCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgTriggerTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.OidColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TgNameColumn);
        builder.Property(row => row.RelationId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TgRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.IsInternal).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TgIsInternalColumn);
        builder.Property(row => row.EnabledStatus).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TgEnabledColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
    }
}
