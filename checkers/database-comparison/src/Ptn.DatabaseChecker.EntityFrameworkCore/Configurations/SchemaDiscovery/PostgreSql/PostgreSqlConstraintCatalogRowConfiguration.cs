using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_constraint katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class PostgreSqlConstraintCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlConstraintCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlConstraintCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgConstraintTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.OidColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConNameColumn);
        builder.Property(row => row.Type).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConTypeColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
        builder.Property(row => row.TableRelId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.ForeignTableRelId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConFRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.ColumnNumbers).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConKeyColumn);
        builder.Property(row => row.ForeignColumnNumbers).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConFKeyColumn);
        builder.Property(row => row.DeleteAction).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConFDeleteTypeColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
        builder.Property(row => row.UpdateAction).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConFUpdateTypeColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
        builder.Property(row => row.IsValidated).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConValidatedColumn);
        builder.Property(row => row.IsDeferrable).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConDeferrableColumn);
        builder.Property(row => row.IsInitiallyDeferred).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.ConDeferredColumn);
    }
}
