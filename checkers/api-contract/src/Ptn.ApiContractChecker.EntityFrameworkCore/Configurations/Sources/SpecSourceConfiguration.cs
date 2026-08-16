using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Sources;

// islevi: SpecSource aggregate'inin tablo, kolon, tenant index ve dokuman iliskisini tanimlar.
// sistemdeki gorevi: Kaynak benzersizligini ve tarihsel dokuman FK'sini veritabani seviyesinde korur.
public class SpecSourceConfiguration : IEntityTypeConfiguration<SpecSource>
{
    // Kaynak aggregate modelini checker semasina eksiksiz uygular.
    public void Configure(EntityTypeBuilder<SpecSource> builder)
    {
        builder.ToTable(ApiContractCheckerTableNames.SpecSources, ApiContractCheckerDbProperties.CheckerSchema);
        builder.ConfigureByConvention();

        builder.Property(source => source.Name)
            .IsRequired()
            .HasMaxLength(SpecSourceConsts.MaxNameLength);
        builder.Property(source => source.BaseUrl)
            .IsRequired()
            .HasMaxLength(SpecSourceConsts.MaxBaseUrlLength);
        builder.Property(source => source.VaultSecretPath)
            .HasMaxLength(SpecSourceConsts.MaxVaultSecretPathLength);
        builder.Property(source => source.IsActive)
            .IsRequired();

        builder.HasIndex(source => new { source.TenantId, source.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL");

        builder.HasIndex(source => new { source.CreatorId, source.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL")
            .AreNullsDistinct(false);

        builder.HasMany(source => source.Documents)
            .WithOne()
            .HasForeignKey(document => document.SpecSourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(source => source.Documents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
