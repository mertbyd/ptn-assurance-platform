using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Snapshots;

// islevi: SpecSnapshot tablosunun kolonlarini, zaman indexini ve tarihsel FK'larini tanimlar.
// sistemdeki gorevi: Dokuman, degismez icerik ve format referanslarinin silinerek gecmis kaybetmesini engeller.
public class SpecSnapshotConfiguration : IEntityTypeConfiguration<SpecSnapshot>
{
    // Snapshot zaman-serisi modelini checker semasina uygular.
    public void Configure(EntityTypeBuilder<SpecSnapshot> builder)
    {
        builder.ToTable(ApiContractCheckerTableNames.SpecSnapshots, ApiContractCheckerDbProperties.CheckerSchema);
        builder.ConfigureByConvention();

        builder.Property(snapshot => snapshot.ApiVersion)
            .HasMaxLength(SpecSnapshotConsts.MaxApiVersionLength);
        builder.Property(snapshot => snapshot.LastSeenAt)
            .IsRequired();

        builder.HasIndex(snapshot => new { snapshot.TenantId, snapshot.SpecDocumentId, snapshot.CreationTime })
            .IsDescending(false, false, true);

        builder.HasOne<SpecDocument>()
            .WithMany()
            .HasForeignKey(snapshot => snapshot.SpecDocumentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(snapshot => snapshot.SpecContent)
            .WithMany()
            .HasForeignKey(snapshot => snapshot.SpecContentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(snapshot => snapshot.SpecFormat)
            .WithMany()
            .HasForeignKey(snapshot => snapshot.SpecFormatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
