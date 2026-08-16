using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.types katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerTypeCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerTypeCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerTypeCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.TypesTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.UserTypeIdTypesColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.SchemaId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SchemaIdColumn);
        builder.Property(row => row.SystemTypeId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SystemTypeIdColumn);
        builder.Property(row => row.MaxLength).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.MaxLengthColumn);
        builder.Property(row => row.Precision).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.PrecisionColumn);
        builder.Property(row => row.Scale).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ScaleColumn);
        builder.Property(row => row.IsUserDefined).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsUserDefinedColumn);
        builder.Property(row => row.IsTableType).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsTableTypeColumn);
    }
}
