using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.extended_properties satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Kolon MS_Description degerlerini object_id + column_id anahtariyla LINQ'e acar.
internal sealed class SqlServerExtendedPropertyCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerExtendedPropertyCatalogRow>, ICatalogModelConfiguration
{
    // islevi: Extended-property scope anahtari, ad ve sql_variant deger kolonlarini baglar.
    public void Configure(EntityTypeBuilder<SqlServerExtendedPropertyCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.ExtendedPropertiesTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => new { row.Class, row.MajorId, row.MinorId, row.Name });
        builder.Property(row => row.Class).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ExtendedPropertyClassColumn);
        builder.Property(row => row.MajorId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ExtendedPropertyMajorIdColumn);
        builder.Property(row => row.MinorId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ExtendedPropertyMinorIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.Value).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ExtendedPropertyValueColumn).HasColumnType(DatabaseMetadataCatalogConstants.SqlServer.SqlVariantColumnType);
    }
}
