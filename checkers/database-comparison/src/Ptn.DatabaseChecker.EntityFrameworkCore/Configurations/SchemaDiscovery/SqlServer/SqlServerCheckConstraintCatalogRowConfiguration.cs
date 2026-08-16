using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.check_constraints katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerCheckConstraintCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerCheckConstraintCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerCheckConstraintCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.CheckConstraintsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.ParentObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.CheckParentObjectIdColumn);
        builder.Property(row => row.Definition).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.DefinitionColumn);
        builder.Property(row => row.IsMsShipped).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsMsShippedColumn);
        builder.Property(row => row.IsDisabled).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsDisabledColumn);
        builder.Property(row => row.IsNotTrusted).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsNotTrustedColumn);
    }
}
