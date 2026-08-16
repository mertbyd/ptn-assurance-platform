using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.PostgreSql;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.PostgreSql;

// islevi: pg_sequence katalog satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca PostgreSqlCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class PostgreSqlSequenceCatalogRowConfiguration
    : IEntityTypeConfiguration<PostgreSqlSequenceCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<PostgreSqlSequenceCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.PostgreSql.PgSequenceTable, DatabaseMetadataCatalogConstants.PostgreSql.PgCatalogSchema);
        builder.HasKey(row => row.SequenceRelId);
        builder.Property(row => row.SequenceRelId).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SeqRelIdColumn).HasColumnType(DatabaseMetadataCatalogConstants.PostgreSql.OidColumnType);
        builder.Property(row => row.StartValue).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SeqStartColumn);
        builder.Property(row => row.Increment).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SeqIncrementColumn);
        builder.Property(row => row.MaximumValue).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SeqMaxColumn);
        builder.Property(row => row.MinimumValue).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SeqMinColumn);
        builder.Property(row => row.CacheValue).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SeqCacheColumn);
        builder.Property(row => row.IsCycling).HasColumnName(DatabaseMetadataCatalogConstants.PostgreSql.SeqCycleColumn);
    }
}
