using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.indexes katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerIndexCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerIndexCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerIndexCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.IndexesTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => new { row.ObjectId, row.IndexId });
        builder.Property(row => row.ObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.IndexId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IndexIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.IsPrimaryKey).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsPrimaryKeyColumn);
        builder.Property(row => row.IsUnique).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsUniqueColumn);
        builder.Property(row => row.IsUniqueConstraint).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsUniqueConstraintColumn);
        builder.Property(row => row.IndexType).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IndexTypeColumn);
        builder.Property(row => row.FilterDefinition).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.FilterDefinitionColumn);
        builder.Property(row => row.IsDisabled).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsDisabledColumn);
    }
}
