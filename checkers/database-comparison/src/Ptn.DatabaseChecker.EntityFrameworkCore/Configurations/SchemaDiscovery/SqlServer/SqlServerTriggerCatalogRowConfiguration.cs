using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.triggers katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerTriggerCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerTriggerCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerTriggerCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.TriggersTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.ParentObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ParentIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.IsMsShipped).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsMsShippedColumn);
        builder.Property(row => row.IsDisabled).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsDisabledColumn);
    }
}
