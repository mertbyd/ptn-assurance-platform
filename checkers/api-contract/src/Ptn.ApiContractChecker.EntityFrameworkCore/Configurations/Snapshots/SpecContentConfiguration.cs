using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Snapshots;

// islevi: Degismez SpecContent tablosunun hash, icerik ve tenant dedup indexlerini tanimlar.
// sistemdeki gorevi: Ayni tenant+raw hash iceriginin eszamanli cekimlerde bile tek satir kalmasini saglar.
public class SpecContentConfiguration : IEntityTypeConfiguration<SpecContent>
{
    // Icerik-adresli kalici modeli checker semasina uygular.
    public void Configure(EntityTypeBuilder<SpecContent> builder)
    {
        builder.ToTable(ApiContractCheckerTableNames.SpecContents, ApiContractCheckerDbProperties.CheckerSchema);
        builder.ConfigureByConvention();

        builder.Property(content => content.RawHash)
            .IsRequired()
            .HasColumnType(SpecContentConsts.HashColumnType)
            .HasMaxLength(SpecContentConsts.HashLength);
        builder.Property(content => content.CanonicalHash)
            .IsRequired()
            .HasColumnType(SpecContentConsts.HashColumnType)
            .HasMaxLength(SpecContentConsts.HashLength);
        builder.Property(content => content.Content)
            .IsRequired()
            .HasColumnType(SpecContentConsts.ContentColumnType);
        builder.Property(content => content.ByteSize)
            .IsRequired();
        builder.Property(content => content.MediaType)
            .IsRequired()
            .HasMaxLength(SpecContentConsts.MaxMediaTypeLength);

        builder.HasIndex(content => new { content.TenantId, content.RawHash })
            .IsUnique()
            .AreNullsDistinct(false);
        builder.HasIndex(content => new { content.TenantId, content.CanonicalHash });
    }
}
