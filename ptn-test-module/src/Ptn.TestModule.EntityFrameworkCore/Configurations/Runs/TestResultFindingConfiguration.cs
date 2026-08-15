using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Runs;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Runs;

// islevi: TestResultFinding cocuk entity'sini DBML kolon, uzunluk ve indekslerine baglar.
// sistemdeki gorevi: test_run.test_result_findings tablosunun tenant-aware EF Core sema sahibidir.
/// <summary>
/// Test sonucu bulgusunun EF Core ve PostgreSQL eslemesini tanimlar.
/// </summary>
public sealed class TestResultFindingConfiguration : IEntityTypeConfiguration<TestResultFinding>
{
    // ABP creation audit alanlarindan sonra tum bulgu ve sorgu indeksi sozlesmesini uygular.
    /// <summary>TestResultFinding alanlarini ve kararli indekslerini yapilandirir.</summary>
    public void Configure(EntityTypeBuilder<TestResultFinding> builder)
    {
        builder.ToTable(TestModuleTableNames.ResultFindings, TestModuleDbProperties.RunSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.TestRunResultId).IsRequired();
        builder.Property(entity => entity.Ordinal).IsRequired();
        builder.Property(entity => entity.Fingerprint).IsRequired().HasMaxLength(TestResultFindingConsts.FingerprintLength);
        builder.Property(entity => entity.SourceCheckerCode).IsRequired().HasMaxLength(TestResultFindingConsts.MaxKindCodeLength);
        builder.Property(entity => entity.ComparisonKindCode).IsRequired().HasMaxLength(TestResultFindingConsts.MaxKindCodeLength);
        builder.Property(entity => entity.RuleRef).HasMaxLength(TestResultFindingConsts.MaxRuleRefLength);
        builder.Property(entity => entity.Location).IsRequired().HasMaxLength(TestResultFindingConsts.MaxLocationLength);
        builder.Property(entity => entity.TargetDisplayName).HasMaxLength(TestResultFindingConsts.MaxTargetDisplayNameLength);
        builder.Property(entity => entity.Message).IsRequired().HasMaxLength(TestResultFindingConsts.MaxMessageLength);
        builder.Property(entity => entity.ExpectedValue).HasMaxLength(TestResultFindingConsts.MaxValueLength);
        builder.Property(entity => entity.ObservedValue).HasMaxLength(TestResultFindingConsts.MaxValueLength);
        builder.Property(entity => entity.EvidenceSummary).HasMaxLength(TestResultFindingConsts.MaxEvidenceSummaryLength);
        builder.HasIndex(entity => new { entity.TestRunResultId, entity.Ordinal })
            .IsUnique()
            .HasDatabaseName(TestResultFindingConsts.OrderIndexName);
        builder.HasIndex(entity => entity.Location).HasDatabaseName(TestResultFindingConsts.LocationIndexName);
        builder.HasIndex(entity => entity.RuleRef).HasDatabaseName(TestResultFindingConsts.RuleIndexName);
        builder.HasIndex(entity => entity.SourceCheckerCode).HasDatabaseName(TestResultFindingConsts.SourceIndexName);
        builder.HasIndex(entity => entity.Fingerprint).HasDatabaseName(TestResultFindingConsts.FingerprintIndexName);
    }
}
