using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_attrdef katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class PostgreSqlAttributeDefaultCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlAttributeDefaultCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlAttributeDefaultCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgAttributeDefaultTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => new { row.RelationId, row.ColumnNumber });
        builder.Property(row => row.RelationId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AdRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.ColumnNumber).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AdNumColumn);
        builder.Property(row => row.BinaryExpression).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.AdBinColumn);
    }
}
