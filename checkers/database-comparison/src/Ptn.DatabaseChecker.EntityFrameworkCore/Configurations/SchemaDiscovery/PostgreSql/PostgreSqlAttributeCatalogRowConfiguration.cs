using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_attribute satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
public class PostgreSqlAttributeCatalogRowConfiguration : IEntityTypeConfiguration<PostgreSqlAttributeCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlAttributeCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgAttributeTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);

        builder.HasKey(x => new { x.RelationId, x.ColumnNumber });

        builder.Property(x => x.RelationId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(x => x.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttNameColumn);
        builder.Property(x => x.TypeId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttTypIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(x => x.ColumnNumber).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttNumColumn);
        builder.Property(x => x.IsNotNull).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttNotNullColumn);
        builder.Property(x => x.HasDefault).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttHasDefColumn);
        builder.Property(x => x.IsDropped).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttIsDroppedColumn);
        builder.Property(x => x.TypeModifier).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttTypModColumn);
        builder.Property(x => x.Identity).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttIdentityColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
        builder.Property(x => x.Generated).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttGeneratedColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
        builder.Property(x => x.CollationId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AttCollationColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
    }
}
