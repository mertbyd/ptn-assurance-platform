using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_index katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class PostgreSqlIndexCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlIndexCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlIndexCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgIndexTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.IndexRelId);
        builder.Property(row => row.IndexRelId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.IndRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.TableRelId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.IndTableRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.IsUnique).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.IndIsUniqueColumn);
        builder.Property(row => row.IsPrimary).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.IndIsPrimaryColumn);
        builder.Property(row => row.NumberOfKeyColumns).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.IndNKeyAttsColumn);
        builder.Property(row => row.ColumnNumbers).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.IndKeyColumn);
        builder.Property(row => row.PredicateExpression).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.IndPredicateColumn);
    }
}
