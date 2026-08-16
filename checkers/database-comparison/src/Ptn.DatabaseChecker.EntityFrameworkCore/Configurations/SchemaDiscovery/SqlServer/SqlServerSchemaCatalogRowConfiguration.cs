using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: sys.schemas katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerSchemaCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerSchemaCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerSchemaCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.SchemasTable, DatabaseMetadataCatalogConstants.SqlServer.SystemSchema);
        builder.HasKey(row => row.Id);
        builder.Property(row => row.Id).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SchemaIdColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.NameColumn);
    }
}
