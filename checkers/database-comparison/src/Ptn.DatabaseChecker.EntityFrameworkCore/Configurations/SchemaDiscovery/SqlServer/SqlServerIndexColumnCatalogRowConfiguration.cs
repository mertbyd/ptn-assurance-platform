using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.index_columns katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerIndexColumnCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerIndexColumnCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerIndexColumnCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.IndexColumnsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => new { row.ObjectId, row.IndexId, row.ColumnId });
        builder.Property(row => row.ObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.IndexId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IndexIdColumn);
        builder.Property(row => row.ColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IndexColumnIdColumn);
        builder.Property(row => row.KeyOrdinal).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.KeyOrdinalColumn);
        builder.Property(row => row.IsIncludedColumn).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsIncludedColumnColumn);
    }
}
