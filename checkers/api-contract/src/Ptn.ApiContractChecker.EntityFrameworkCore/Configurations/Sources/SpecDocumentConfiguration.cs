using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Sources;

// islevi: SpecDocument aggregate cocugunun tablo, kolon ve kaynak-ici benzersizligini tanimlar.
// sistemdeki gorevi: Dokuman adinin ayni SpecSource altinda tek olmasini veritabani seviyesinde korur.
public class SpecDocumentConfiguration : IEntityTypeConfiguration<SpecDocument>
{
    // Aggregate cocugu modelini checker semasina uygular.
    public void Configure(EntityTypeBuilder<SpecDocument> builder)
    {
        builder.ToTable(ApiContractCheckerTableNames.SpecDocuments, ApiContractCheckerDbProperties.CheckerSchema);
        builder.ConfigureByConvention();

        builder.Property(document => document.DocumentName)
            .IsRequired()
            .HasMaxLength(SpecDocumentConsts.MaxDocumentNameLength);
        builder.Property(document => document.Path)
            .IsRequired()
            .HasMaxLength(SpecDocumentConsts.MaxPathLength);
        builder.Property(document => document.IsActive)
            .IsRequired();
        builder.Property(document => document.IsMonitored)
            .IsRequired();
        builder.Property(document => document.LastFetchOutcome)
            .HasMaxLength(SpecDocumentConsts.MaxFetchOutcomeLength);

        builder.HasIndex(document => new { document.SpecSourceId, document.DocumentName })
            .IsUnique();

        // Worker taramasinin tek predicate'i budur; vadesi gelmis satirlar tablo taramasi olmadan bulunur.
        builder.HasIndex(document => new { document.IsMonitored, document.NextCheckAt });
    }
}
