using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Runs;

// islevi: ContractCheckRun tablosunun baslik, owned JSON, index ve tarihsel FK eslemesini tanimlar.
// sistemdeki gorevi: Liste basligini hafif tutarken bulgu govdesini tek jsonb kolonunda ve snapshot gecmisini Restrict ile korur.
public class ContractCheckRunConfiguration : IEntityTypeConfiguration<ContractCheckRun>
{
    // Run basligi ve owned bulgu modelini checker semasina uygular.
    public void Configure(EntityTypeBuilder<ContractCheckRun> builder)
    {
        builder.ToTable(ApiContractCheckerTableNames.ContractCheckRuns, ApiContractCheckerDbProperties.CheckerSchema);
        builder.ConfigureByConvention();

        builder.Property(run => run.ErrorMessage)
            .HasMaxLength(ContractCheckRunConsts.MaxErrorMessageLength);
        builder.Property(run => run.BreakingCount)
            .IsRequired();
        builder.Property(run => run.NonBreakingCount)
            .IsRequired();
        builder.Property(run => run.DocsOnlyCount)
            .IsRequired();

        builder.HasIndex(run => new { run.TenantId, run.CreationTime })
            .IsDescending(false, true);
        builder.HasIndex(run => new { run.TenantId, run.TargetSnapshotId });

        builder.HasOne<SpecSnapshot>()
            .WithMany()
            .HasForeignKey(run => run.BaseSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SpecSnapshot>()
            .WithMany()
            .HasForeignKey(run => run.TargetSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CheckRunStatus>()
            .WithMany()
            .HasForeignKey(run => run.CheckRunStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(run => run.Findings, findings =>
        {
            findings.ToJson(ContractCheckRunConsts.FindingsJsonColumnName);
            findings.OwnsMany(value => value.Items, item =>
            {
                item.OwnsOne(value => value.Address);
            });
        });
    }
}
