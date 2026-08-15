using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Lookups;

// islevi: TestOutcomeStatus lookup'unu kararli tablo sabitine baglar ve build politikasi kolonunu ekler.
// sistemdeki gorevi: Ortak lookup eslemesinin uzerine, hukmun build'i kirip kirmadigini tasiyan kolonu kurar (ADR-0016 §F).
public class TestOutcomeStatusConfiguration : LookupEntityConfigurationBase<TestOutcomeStatus>
{
    protected override string TableName => TestModuleTableNames.OutcomeStatuses;

    // Ortak lookup eslemesine build politikasi kolonunu ekler; politika koda degil veriye baglidir.
    public override void Configure(EntityTypeBuilder<TestOutcomeStatus> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.BreaksBuild)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
