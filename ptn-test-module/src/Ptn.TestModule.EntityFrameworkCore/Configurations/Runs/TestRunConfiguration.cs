using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Runs;

// islevi: TestRun aggregate'ini DBML kolon, uzunluk, indeks ve Restrict iliskilerine baglar.
// sistemdeki gorevi: test_run.test_runs tablosunun tek EF Core sema sahibidir.
/// <summary>
/// Test kosum kaydinin EF Core ve PostgreSQL eslemesini tanimlar.
/// </summary>
public sealed class TestRunConfiguration : IEntityTypeConfiguration<TestRun>
{
    // ABP convention alanlarindan sonra DBML'nin tum kosum basligi sozlesmesini uygular.
    /// <summary>TestRun alanlarini, indekslerini ve navigation'siz FK baglarini yapilandirir.</summary>
    public void Configure(EntityTypeBuilder<TestRun> builder)
    {
        builder.ToTable(TestModuleTableNames.Runs, TestModuleDbProperties.RunSchema);
        builder.ConfigureByConvention();
        builder.Property(entity => entity.TestKey).IsRequired().HasMaxLength(TestRunConsts.MaxTestKeyLength);
        builder.Property(entity => entity.HistoryId).HasMaxLength(TestRunConsts.HashLength);
        builder.Property(entity => entity.EnvironmentKey).IsRequired().HasMaxLength(TestRunConsts.MaxEnvironmentKeyLength);
        builder.Property(entity => entity.RunStatusId).IsRequired();
        builder.Property(entity => entity.TriggerKindId).IsRequired();
        builder.Property(entity => entity.TriggerRef).HasMaxLength(TestRunConsts.MaxTriggerRefLength);
        builder.Property(entity => entity.TraceId).HasMaxLength(TestRunConsts.TraceIdLength);
        builder.Property(entity => entity.SpecFingerprint).HasMaxLength(TestRunConsts.HashLength);
        builder.Property(entity => entity.DbSchemaFingerprint).HasMaxLength(TestRunConsts.HashLength);
        builder.Property(entity => entity.RunnerRef).HasMaxLength(TestRunConsts.MaxRunnerRefLength);
        builder.Property(entity => entity.IsDryRun).IsRequired().HasDefaultValue(false);
        builder.Property(entity => entity.HarBlobName).HasMaxLength(TestRunConsts.MaxHarBlobNameLength);
        builder.HasIndex(entity => entity.TestKey).HasDatabaseName(TestRunConsts.TestIndexName);
        builder.HasIndex(entity => entity.HistoryId).HasDatabaseName(TestRunConsts.TrendIndexName);
        builder.HasIndex(entity => new { entity.RunStatusId, entity.StartedAt }).HasDatabaseName(TestRunConsts.StaleIndexName);
        builder.HasIndex(entity => entity.TraceId).HasDatabaseName(TestRunConsts.TraceIndexName);
        builder.HasOne<TestScenario>()
            .WithMany()
            .HasForeignKey(entity => entity.ScenarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TestRunStatus>()
            .WithMany()
            .HasForeignKey(entity => entity.RunStatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TestTriggerKind>()
            .WithMany()
            .HasForeignKey(entity => entity.TriggerKindId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
