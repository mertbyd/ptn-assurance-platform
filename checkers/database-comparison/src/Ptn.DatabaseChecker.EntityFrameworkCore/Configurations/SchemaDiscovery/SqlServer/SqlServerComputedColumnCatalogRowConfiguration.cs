using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.computed_columns satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Computed ifade ve persisted bilgisini kolon snapshot okumasina acar.
internal sealed class SqlServerComputedColumnCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerComputedColumnCatalogRow>, ICatalogModelConfiguration
{
    // islevi: sys.computed_columns kolon anahtari, definition ve is_persisted alanlarini baglar.
    public void Configure(EntityTypeBuilder<SqlServerComputedColumnCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.ComputedColumnsTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => new { row.ObjectId, row.ColumnId });
        builder.Property(row => row.ObjectId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ObjectIdColumn);
        builder.Property(row => row.ColumnId).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.ColumnIdColumn);
        builder.Property(row => row.Definition).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.DefinitionColumn);
        builder.Property(row => row.IsPersisted).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.IsPersistedColumn);
    }
}
