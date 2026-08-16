using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_description satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Kolon comment'lerini relation oid ve attnum anahtariyla LINQ'e acar.
internal sealed class PostgreSqlDescriptionCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlDescriptionCatalogRow>, ICatalogModelConfiguration
{
    // islevi: pg_description birlesik anahtar ve aciklama kolonlarini salt-okunur modele baglar.
    public void Configure(EntityTypeBuilder<PostgreSqlDescriptionCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgDescriptionTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => new { row.ObjectId, row.CatalogId, row.SubObjectId });
        builder.Property(row => row.ObjectId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DescriptionObjectIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.CatalogId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DescriptionCatalogIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.SubObjectId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DescriptionSubObjectIdColumn);
        builder.Property(row => row.Description).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DescriptionTextColumn);
    }
}
