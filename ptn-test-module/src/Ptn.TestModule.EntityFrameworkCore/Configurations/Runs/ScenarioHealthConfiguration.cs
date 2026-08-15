using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Runs;

// islevi: Senaryo saglik materialized view'ini anahtarsiz ve salt-okunur EF tipine baglar.
// sistemdeki gorevi: View govdesi migration'daki el yazimi SQL'e ait oldugu icin kolon adlari burada acikca pinlenir.
public sealed class ScenarioHealthConfiguration : IEntityTypeConfiguration<ScenarioHealth>
{
    // View'i EF modeline tablo uretmeden tanitir; ConfigureByConvention cagrilmaz, tip ABP entity'si degildir.
    public void Configure(EntityTypeBuilder<ScenarioHealth> builder)
    {
        builder.HasNoKey();
        builder.ToView(ScenarioHealthConsts.ViewName, TestModuleDbProperties.RunSchema);
        builder.Property(entity => entity.TenantKey).HasColumnName(ScenarioHealthConsts.Columns.TenantKey);
        builder.Property(entity => entity.ScenarioKey).HasColumnName(ScenarioHealthConsts.Columns.ScenarioKey);
        builder.Property(entity => entity.TotalRunCount).HasColumnName(ScenarioHealthConsts.Columns.TotalRunCount);
        builder.Property(entity => entity.PassedRunCount).HasColumnName(ScenarioHealthConsts.Columns.PassedRunCount);
        builder.Property(entity => entity.FailedRunCount).HasColumnName(ScenarioHealthConsts.Columns.FailedRunCount);
        builder.Property(entity => entity.HistoryCount).HasColumnName(ScenarioHealthConsts.Columns.HistoryCount);
        builder.Property(entity => entity.FlakyHistoryCount).HasColumnName(ScenarioHealthConsts.Columns.FlakyHistoryCount);
        builder.Property(entity => entity.PassRatio).HasColumnName(ScenarioHealthConsts.Columns.PassRatio);
        builder.Property(entity => entity.FlakyRatio).HasColumnName(ScenarioHealthConsts.Columns.FlakyRatio);
        builder.Property(entity => entity.P95DurationMs).HasColumnName(ScenarioHealthConsts.Columns.P95DurationMs);
        builder.Property(entity => entity.LastRunAt).HasColumnName(ScenarioHealthConsts.Columns.LastRunAt);
    }
}
