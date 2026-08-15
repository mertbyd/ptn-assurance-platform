using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Runs;

// islevi: TestRunResult aggregate'ini DBML kolon, jsonb, indeks ve cascade iliskilerine baglar.
// sistemdeki gorevi: test_run.test_run_results tablosunun tek EF Core sema sahibidir.
/// <summary>
/// Test kosum sonucunun EF Core ve PostgreSQL eslemesini tanimlar.
/// </summary>
public sealed class TestRunResultConfiguration : IEntityTypeConfiguration<TestRunResult>
{
    // ABP convention alanlarindan sonra terminal hukum ve aggregate cocuk sozlesmesini uygular.
    /// <summary>TestRunResult alanlarini, indekslerini ve FK silme davranislarini yapilandirir.</summary>
    public void Configure(EntityTypeBuilder<TestRunResult> builder)
    {
        builder.ToTable(TestModuleTableNames.RunResults, TestModuleDbProperties.RunSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.TestRunId).IsRequired();
        builder.Property(entity => entity.Attempt).IsRequired().HasDefaultValue(1);
        builder.Property(entity => entity.OutcomeStatusId).IsRequired();
        builder.Property(entity => entity.DurationMs).IsRequired();
        builder.Property(entity => entity.ErrorCode).HasMaxLength(TestRunResultConsts.MaxErrorCodeLength);
        builder.Property(entity => entity.Detail).HasMaxLength(TestRunResultConsts.MaxDetailLength);
        builder.Property(entity => entity.FailedStepName).HasMaxLength(TestRunResultConsts.MaxStepNameLength);
        builder.Property(entity => entity.FailedStepPath).HasMaxLength(TestRunResultConsts.MaxStepPathLength);
        builder.Property(entity => entity.TakenBranchPath).HasMaxLength(TestRunResultConsts.MaxBranchPathLength);
        builder.Property(entity => entity.DiagnosisReport).HasColumnType("jsonb");
        builder.Property(entity => entity.CtrfBlobName).HasMaxLength(RunArtifactConsts.MaxBlobNameLength);
        builder.Property(entity => entity.JUnitBlobName)
            .HasColumnName(RunArtifactConsts.JUnitBlobColumnName)
            .HasMaxLength(RunArtifactConsts.MaxBlobNameLength);
        builder.Property(entity => entity.SarifBlobName).HasMaxLength(RunArtifactConsts.MaxBlobNameLength);
        builder.HasIndex(entity => new { entity.TestRunId, entity.Attempt })
            .IsUnique()
            .HasDatabaseName(TestRunResultConsts.AttemptIndexName);
        builder.HasIndex(entity => entity.ErrorCode)
            .HasDatabaseName(TestRunResultConsts.ErrorIndexName);
        builder.HasOne<TestRun>()
            .WithMany()
            .HasForeignKey(entity => entity.TestRunId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<TestOutcomeStatus>()
            .WithMany()
            .HasForeignKey(entity => entity.OutcomeStatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TestFailureCategory>()
            .WithMany()
            .HasForeignKey(entity => entity.FailureCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(entity => entity.Findings)
            .WithOne()
            .HasForeignKey(entity => entity.TestRunResultId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(entity => entity.Findings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
