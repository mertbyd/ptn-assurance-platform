using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.identity_columns satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Identity seed/increment sql_variant degerlerini kolon snapshot okumasina acar.
internal sealed class SqlServerIdentityColumnCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerIdentityColumnCatalogRow>, ICatalogModelConfiguration
{
    // islevi: sys.identity_columns kolon anahtari ile seed/increment alanlarini baglar.
    public void Configure(EntityTypeBuilder<SqlServerIdentityColumnCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.IdentityColumnsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => new { row.ObjectId, row.ColumnId });
        builder.Property(row => row.ObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.ColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ColumnIdColumn);
        builder.Property(row => row.SeedValue).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SeedValueColumn).HasColumnType(DatabaseMetadataCatalogConstants.SqlServer.SqlVariantColumnType);
        builder.Property(row => row.IncrementValue).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IncrementValueColumn).HasColumnType(DatabaseMetadataCatalogConstants.SqlServer.SqlVariantColumnType);
    }
}
