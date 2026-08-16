using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: ComparisonRunStatus lookup tablosunun sema/kolon/index eslemesini tanimlar.
// sistemdeki gorevi: comparison.comparison_run_statuses tablosunu, kolon uzunluklarini (LookupConsts) ve Code uzerindeki benzersiz index'i EF'e bildirir.
public class ComparisonRunStatusConfiguration : IEntityTypeConfiguration<ComparisonRunStatus>
{
    public void Configure(EntityTypeBuilder<ComparisonRunStatus> builder)
    {
        builder.ToTable(DatabaseCheckerTableNames.ComparisonRunStatuses, DatabaseCheckerDbProperties.LookupsSchema);
        builder.ConfigureByConvention();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(LookupConsts.MaxCodeLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(LookupConsts.MaxNameLength);
        builder.Property(x => x.Description).HasMaxLength(LookupConsts.MaxDescriptionLength);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
