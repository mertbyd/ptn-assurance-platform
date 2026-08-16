using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_type satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
public class PostgreSqlTypeCatalogRowConfiguration : IEntityTypeConfiguration<PostgreSqlTypeCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlTypeCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgTypeTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.OidColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(x => x.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TypNameColumn);
        builder.Property(x => x.NamespaceId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TypNamespaceColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(x => x.TypeKind).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TypTypeColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(
                str => string.IsNullOrEmpty(str) ? '\0' : str[0],
                ch => ch == '\0' ? string.Empty : ch.ToString());
        builder.Property(x => x.BaseTypeId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.TypBaseTypeColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
    }
}
