using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Lookups;

public class DatabaseEngineConfiguration : IEntityTypeConfiguration<DatabaseEngine>
{
    public void Configure(EntityTypeBuilder<DatabaseEngine> builder)
    {
        builder.ToTable(DatabaseCheckerTableNames.DatabaseEngines, DatabaseCheckerDbProperties.LookupsSchema);
        builder.ConfigureByConvention();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(LookupConsts.MaxCodeLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(LookupConsts.MaxNameLength);
        builder.Property(x => x.Description).HasMaxLength(LookupConsts.MaxDescriptionLength);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
