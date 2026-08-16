using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.foreign_key_columns katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerForeignKeyColumnCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerForeignKeyColumnCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerForeignKeyColumnCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.ForeignKeyColumnsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => new { row.ConstraintObjectId, row.ConstraintColumnId });
        builder.Property(row => row.ConstraintObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ConstraintObjectIdColumn);
        builder.Property(row => row.ConstraintColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ConstraintColumnIdColumn);
        builder.Property(row => row.ParentObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ParentObjectIdColumn);
        builder.Property(row => row.ParentColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ParentColumnIdColumn);
        builder.Property(row => row.ReferencedObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ReferencedObjectIdColumn);
        builder.Property(row => row.ReferencedColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ReferencedColumnIdColumn);
    }
}
