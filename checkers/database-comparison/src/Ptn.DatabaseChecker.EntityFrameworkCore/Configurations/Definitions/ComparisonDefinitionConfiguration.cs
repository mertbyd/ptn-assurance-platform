using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Definitions;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations.Definitions;

// islevi: ComparisonDefinition tablosunun sema, kolon, index ve FK eslemelerini tanimlar.
// sistemdeki gorevi: Kaynak/hedef baglanti ve karsilastirma modunu FK ile korur; calistirma kapsamlarini kalici modele dahil etmez.
public class ComparisonDefinitionConfiguration : IEntityTypeConfiguration<ComparisonDefinition>
{
    public void Configure(EntityTypeBuilder<ComparisonDefinition> builder)
    {
        builder.ToTable(DatabaseCheckerTableNames.ComparisonDefinitions, DatabaseCheckerDbProperties.DefinitionsSchema);
        builder.ConfigureByConvention();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(ComparisonDefinitionConsts.MaxNameLength);
        builder.Property(x => x.Description).HasMaxLength(ComparisonDefinitionConsts.MaxDescriptionLength);
        builder.Property(x => x.SourceRoleCode)
            .IsRequired()
            .HasMaxLength(ComparisonDefinitionConsts.MaxSourceRoleCodeLength)
            .HasDefaultValue(ComparisonSideRoleCodes.Reference);

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

        builder.HasIndex(definition => new { definition.TenantId, definition.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL");

        builder.HasIndex(definition => new { definition.CreatorId, definition.Name })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL")
            .AreNullsDistinct(false);
    }
}
