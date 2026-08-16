using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_depend satirinin external okuma context'indeki dependency kolon eslemesini tanimlar.
// sistemdeki gorevi: Identity kolonun sahip sequence bagini ham SQL olmadan LINQ ile kurar.
internal sealed class PostgreSqlDependCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlDependCatalogRow>, ICatalogModelConfiguration
{
    // islevi: pg_depend birlesik anahtar ve referans kolonlarini salt-okunur modele baglar.
    public void Configure(EntityTypeBuilder<PostgreSqlDependCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgDependTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => new { row.CatalogId, row.ObjectId, row.ObjectSubId, row.ReferencedCatalogId, row.ReferencedObjectId, row.ReferencedObjectSubId });
        builder.Property(row => row.CatalogId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DependClassIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.ObjectId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DependObjectIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.ObjectSubId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DependObjectSubIdColumn);
        builder.Property(row => row.ReferencedCatalogId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DependReferencedClassIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.ReferencedObjectId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DependReferencedObjectIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.ReferencedObjectSubId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DependReferencedObjectSubIdColumn);
        builder.Property(row => row.DependencyType).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.DependTypeColumn)
            .HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.InternalCharColumnType)
            .HasConversion(str => string.IsNullOrEmpty(str) ? '\0' : str[0], ch => ch == '\0' ? string.Empty : ch.ToString());
    }
}
