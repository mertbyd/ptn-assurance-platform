using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.columns katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerColumnCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerColumnCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerColumnCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.ColumnsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => new { row.ObjectId, row.ColumnId });
        builder.Property(row => row.ObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.ColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ColumnIdColumn);
        builder.Property(row => row.SystemTypeId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SystemTypeIdColumn);
        builder.Property(row => row.UserTypeId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.UserTypeIdColumn);
        builder.Property(row => row.MaxLength).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.MaxLengthColumn);
        builder.Property(row => row.Precision).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.PrecisionColumn);
        builder.Property(row => row.Scale).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ScaleColumn);
        builder.Property(row => row.IsNullable).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsNullableColumn);
        builder.Property(row => row.IsIdentity).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsIdentityColumn);
        builder.Property(row => row.IsComputed).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsComputedColumn);
        builder.Property(row => row.CollationName).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.CollationNameColumn);
    }
}
