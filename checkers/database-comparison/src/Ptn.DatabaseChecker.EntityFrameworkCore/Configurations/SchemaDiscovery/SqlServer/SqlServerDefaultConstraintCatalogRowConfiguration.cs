using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.default_constraints katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerDefaultConstraintCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerDefaultConstraintCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerDefaultConstraintCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.DefaultConstraintsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.ParentObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ParentObjectIdColumn);
        builder.Property(row => row.ParentColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.DefaultParentColumnIdColumn);
        builder.Property(row => row.Definition).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.DefinitionColumn);
    }
}
