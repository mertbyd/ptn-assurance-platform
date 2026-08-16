using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.objects katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerObjectCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerObjectCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerObjectCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.ObjectsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.SchemaId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SchemaIdColumn);
        builder.Property(row => row.Type).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.TypeColumn);
        builder.Property(row => row.IsMsShipped).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsMsShippedColumn);
    }
}
