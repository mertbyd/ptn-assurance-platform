using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_database satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Migration-disi katalog context'inde veritabani collation metadata'sini LINQ'e acar.
internal sealed class PostgreSqlDatabaseCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlDatabaseCatalogRow>, ICatalogModelConfiguration
{
    // islevi: pg_database ad, oid ve datcollate kolonlarini salt-okunur modele baglar.
    public void Configure(EntityTypeBuilder<PostgreSqlDatabaseCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgDatabaseTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.OidColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DatNameColumn);
        builder.Property(row => row.CollationName).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DatCollateColumn);
    }
}
