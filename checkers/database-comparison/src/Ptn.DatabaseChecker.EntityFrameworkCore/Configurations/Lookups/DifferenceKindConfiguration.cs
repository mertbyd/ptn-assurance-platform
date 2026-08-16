using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: DifferenceKind lookup tablosunun sema/kolon/index eslemesini tanimlar.
// sistemdeki gorevi: comparison.difference_kinds tablosunu, kolon uzunluklarini (LookupConsts) ve Code uzerindeki benzersiz index'i EF'e bildirir.
public class DifferenceKindConfiguration : IEntityTypeConfiguration<DifferenceKind>
{
    public void Configure(EntityTypeBuilder<DifferenceKind> builder)
    {
        builder.ToTable(DatabaseCheckerTableNames.DifferenceKinds, DatabaseCheckerDbProperties.LookupsSchema);
        builder.ConfigureByConvention();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(LookupConsts.MaxCodeLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(LookupConsts.MaxNameLength);
        builder.Property(x => x.Description).HasMaxLength(LookupConsts.MaxDescriptionLength);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
