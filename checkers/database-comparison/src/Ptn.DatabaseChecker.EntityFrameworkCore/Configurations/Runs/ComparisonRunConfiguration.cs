using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Runs;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Runs;

// islevi: ComparisonRun tablosunun sema, kolon, index ve FK eslemelerini tanimlar.
// sistemdeki gorevi: Kalici calistirma kaydinin tarif, baglanti, mod ve durum snapshot baglarini veritabani seviyesinde korur.
public class ComparisonRunConfiguration : IEntityTypeConfiguration<ComparisonRun>
{
    public void Configure(EntityTypeBuilder<ComparisonRun> builder)
    {
        builder.ToTable(DatabaseCheckerTableNames.ComparisonRuns, DatabaseCheckerDbProperties.RunsSchema);
        builder.ConfigureByConvention();

        builder.Property(x => x.ErrorMessage).HasMaxLength(ComparisonRunConsts.MaxErrorMessageLength);

        builder.HasOne(x => x.Definition)
            .WithMany()
            .HasForeignKey(x => x.ComparisonDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceConnection)
            .WithMany()
            .HasForeignKey(x => x.SourceConnectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetConnection)
            .WithMany()
            .HasForeignKey(x => x.TargetConnectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ComparisonType)
            .WithMany()
            .HasForeignKey(x => x.ComparisonTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ComparisonDefinitionId);
        builder.HasIndex(x => x.CreationTime);

        // Bulgular ve rapor icerikleri owned type olarak jsonb kolonlarina serilestirilir (eski difference/report tablolarinin yerini alir).
        // "JSON yok" ilkesi yalnizca bu donmus payload'lar icin bilincli esnetildi; ic sorgu gerekirse jsonb operatorleri veya Elasticsearch kullanilir.
        builder.OwnsOne(x => x.Findings, findings =>
        {
            findings.ToJson("findings");
            findings.OwnsMany(f => f.SchemaDifferences);
            findings.OwnsMany(f => f.MigrationDifferences);
            findings.OwnsMany(f => f.DataDifferences, data =>
            {
                data.OwnsMany(d => d.RowDifferences, row =>
                {
                    row.OwnsMany(r => r.ValueDifferences);
                });
            });
        });

        builder.OwnsMany(x => x.Reports, nav => nav.ToJson("reports"));
    }
}
