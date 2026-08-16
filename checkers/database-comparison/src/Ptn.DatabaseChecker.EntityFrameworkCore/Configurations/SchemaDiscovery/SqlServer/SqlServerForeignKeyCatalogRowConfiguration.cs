using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.foreign_keys katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerForeignKeyCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerForeignKeyCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerForeignKeyCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.ForeignKeysTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.ParentObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ParentObjectIdColumn);
        builder.Property(row => row.ReferencedObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ReferencedObjectIdColumn);
        builder.Property(row => row.DeleteReferentialAction).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.DeleteReferentialActionColumn);
        builder.Property(row => row.UpdateReferentialAction).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.UpdateReferentialActionColumn);
        builder.Property(row => row.IsDisabled).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsDisabledColumn);
        builder.Property(row => row.IsNotTrusted).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsNotTrustedColumn);
    }
}
