using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.databases satirinin external okuma context'indeki kolon eslemesini tanimlar.
// sistemdeki gorevi: Veritabani collation adini migration-disi katalog LINQ sorgusuna acar.
internal sealed class SqlServerDatabaseCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerDatabaseCatalogRow>, ICatalogModelConfiguration
{
    // islevi: sys.databases id, ad ve collation_name kolonlarini salt-okunur modele baglar.
    public void Configure(EntityTypeBuilder<SqlServerDatabaseCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.DatabasesTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.DatabaseIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
        builder.Property(row => row.CollationName).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.CollationNameColumn);
    }
}
