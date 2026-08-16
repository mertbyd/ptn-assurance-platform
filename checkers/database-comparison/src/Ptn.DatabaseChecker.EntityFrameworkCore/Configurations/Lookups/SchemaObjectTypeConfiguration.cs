using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: SchemaObjectType lookup tablosunun sema/kolon/index eslemesini tanimlar.
// sistemdeki gorevi: comparison.schema_object_types tablosunu, kolon uzunluklarini (LookupConsts) ve Code uzerindeki benzersiz index'i EF'e bildirir.
public class SchemaObjectTypeConfiguration : IEntityTypeConfiguration<SchemaObjectType>
{
    public void Configure(EntityTypeBuilder<SchemaObjectType> builder)
    {
        builder.ToTable(DatabaseCheckerTableNames.SchemaObjectTypes, DatabaseCheckerDbProperties.LookupsSchema);
        builder.ConfigureByConvention();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(LookupConsts.MaxCodeLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(LookupConsts.MaxNameLength);
        builder.Property(x => x.Description).HasMaxLength(LookupConsts.MaxDescriptionLength);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
