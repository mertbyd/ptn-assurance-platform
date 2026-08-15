using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Lookups;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Catalog;

// islevi: TestScenario aggregate'ini DBML kolon, uzunluk, indeks ve Restrict iliskisine baglar.
// sistemdeki gorevi: test_catalog.test_scenarios tablosunun tek EF Core sema sahibidir.
public sealed class TestScenarioConfiguration : IEntityTypeConfiguration<TestScenario>
{
    // ABP convention alanlarindan sonra DBML'nin tum katalog sozlesmesini uygular.
    public void Configure(EntityTypeBuilder<TestScenario> builder)
    {
        builder.ToTable(TestModuleTableNames.Scenarios, TestModuleDbProperties.CatalogSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.ScenarioKey).IsRequired().HasMaxLength(TestScenarioConsts.MaxScenarioKeyLength);
        builder.Property(entity => entity.VersionNo).IsRequired();
        builder.Property(entity => entity.Title).IsRequired().HasMaxLength(TestScenarioConsts.MaxTitleLength);
        builder.Property(entity => entity.Description).HasMaxLength(TestScenarioConsts.MaxDescriptionLength);
        builder.Property(entity => entity.StateId).IsRequired();
        builder.Property(entity => entity.SourceDocument).IsRequired();
        builder.Property(entity => entity.SourceHash).IsRequired().HasMaxLength(TestScenarioConsts.HashLength);
        builder.Property(entity => entity.CompiledDocument).IsRequired();
        builder.Property(entity => entity.CompiledHash).IsRequired().HasMaxLength(TestScenarioConsts.HashLength);
        builder.Property(entity => entity.RulesFingerprint).HasMaxLength(TestScenarioConsts.HashLength);
        builder.Property(entity => entity.SpecFingerprint).HasMaxLength(TestScenarioConsts.HashLength);
        builder.Property(entity => entity.DbSchemaFingerprint).HasMaxLength(TestScenarioConsts.HashLength);
        builder.Property(entity => entity.ProfileFingerprint).HasMaxLength(TestScenarioConsts.HashLength);
        builder.Property(entity => entity.AssertionCount).IsRequired().HasDefaultValue(0);
        builder.Property(entity => entity.DerivabilityCode).HasMaxLength(TestScenarioConsts.MaxDerivabilityCodeLength);
        builder.Property(entity => entity.AuthoredByAgent).IsRequired().HasDefaultValue(false);
        builder.Property(entity => entity.AgentModelRef).HasMaxLength(TestScenarioConsts.MaxAgentModelRefLength);
        builder.Property(entity => entity.ApprovalBoundToHash).HasMaxLength(TestScenarioConsts.HashLength);
        builder.Property(entity => entity.Notes).HasMaxLength(TestScenarioConsts.MaxNotesLength);
        builder.HasIndex(entity => new { entity.ScenarioKey, entity.VersionNo })
            .IsUnique()
            .HasDatabaseName(TestScenarioConsts.VersionIndexName);
        builder.HasIndex(entity => new { entity.ScenarioKey, entity.SourceHash })
            .IsUnique()
            .HasDatabaseName(TestScenarioConsts.ContentIndexName);
        builder.HasIndex(entity => entity.StateId)
            .HasDatabaseName(TestScenarioConsts.StateIndexName);
        builder.Property(entity => entity.QuarantineReason)
            .HasMaxLength(TestScenarioConsts.MaxQuarantineReasonLength);
        builder.HasIndex(entity => entity.QuarantineUntil)
            .HasDatabaseName(TestScenarioConsts.QuarantineIndexName);
        builder.Property(entity => entity.ScheduleCron)
            .HasMaxLength(TestScenarioConsts.MaxScheduleCronLength);
        builder.Property(entity => entity.ScheduleEnabled).IsRequired().HasDefaultValue(false);
        builder.HasIndex(entity => new { entity.ScheduleEnabled, entity.NextRunAt })
            .HasDatabaseName(TestScenarioConsts.ScheduleIndexName);
        builder.HasOne<TestScenarioState>()
            .WithMany()
            .HasForeignKey(entity => entity.StateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
