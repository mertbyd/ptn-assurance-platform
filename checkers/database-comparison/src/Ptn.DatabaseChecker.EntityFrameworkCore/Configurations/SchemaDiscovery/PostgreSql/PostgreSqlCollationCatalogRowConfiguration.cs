using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_collation satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Kolon collation adlari ile provider kodunu migration-disi LINQ sorgularina acar.
internal sealed class PostgreSqlCollationCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlCollationCatalogRow>, ICatalogModelConfiguration
{
    // islevi: pg_collation oid, ad ve collprovider kolonlarini salt-okunur modele baglar.
    public void Configure(EntityTypeBuilder<PostgreSqlCollationCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgCollationTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.OidColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.CollNameColumn);
        builder.Property(row => row.ProviderCode).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.CollProviderColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(str => string.IsNullOrEmpty(str) ? '\0' : str[0], ch => ch == '\0' ? string.Empty : ch.ToString());
    }
}
