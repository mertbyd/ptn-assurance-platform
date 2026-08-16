using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.SchemaDiscovery.SqlServer;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.SchemaDiscovery.SqlServer;

// islevi: INFORMATION_SCHEMA.SEQUENCES satirinin external okuma context'indeki tablo/kolon eslemesini tanimlar.
// sistemdeki gorevi: Esleme ICatalogModelConfiguration filtresiyle yalnizca SqlServerCatalogDbContext'e uygulanir, ana app modeline girmez.
internal sealed class SqlServerSequenceCatalogRowConfiguration
    : IEntityTypeConfiguration<SqlServerSequenceCatalogRow>, ICatalogModelConfiguration
{
    public void Configure(EntityTypeBuilder<SqlServerSequenceCatalogRow> builder)
    {
        builder.ToTable(DatabaseMetadataCatalogConstants.SqlServer.SequencesView, DatabaseMetadataCatalogConstants.SqlServer.InformationSchema);
        builder.HasKey(row => new { row.Schema, row.Name });
        builder.Property(row => row.Schema).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceSchemaColumn);
        builder.Property(row => row.Name).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceNameColumn);
        builder.Property(row => row.DataType).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceDataTypeColumn);
        builder.Property(row => row.StartValue).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceStartValueColumn);
        builder.Property(row => row.MinimumValue).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceMinimumValueColumn);
        builder.Property(row => row.MaximumValue).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceMaximumValueColumn);
        builder.Property(row => row.Increment).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceIncrementColumn);
        builder.Property(row => row.CycleOption).HasColumnName(DatabaseMetadataCatalogConstants.SqlServer.SequenceCycleOptionColumn);
    }
}
