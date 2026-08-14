using Microsoft.EntityFrameworkCore;
using Ptn.TestModule.Entities.Lookups;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore;

// islevi: Test Module'un tek DbContext'idir; senaryo, kosum ve is bilgisi aggregate koklerini tasir.
// sistemdeki gorevi: Checker ve Auth tablolarina FK vermeden yalniz kendi semalarinin modelini kurar.
[ConnectionStringName(TestModuleDbProperties.ConnectionStringName)]
public class TestModuleDbContext : AbpDbContext<TestModuleDbContext>, ITestModuleDbContext
{
    /* Aggregate root basina DbSet buraya eklenir. Ornek:
     * public DbSet<Scenario> Scenarios { get; set; } = null!;
     */

    // Sabit deger listeleri (test_lookup semasi); global referans verisidir, IMultiTenant tasimaz (ADR-0016 §D).
    public DbSet<TestRunStatus> TestRunStatuses { get; set; } = null!;

    public DbSet<TestOutcomeStatus> TestOutcomeStatuses { get; set; } = null!;

    public DbSet<TestFailureCategory> TestFailureCategories { get; set; } = null!;

    public DbSet<TestTriggerKind> TestTriggerKinds { get; set; } = null!;

    public DbSet<TestScenarioState> TestScenarioStates { get; set; } = null!;

    public TestModuleDbContext(DbContextOptions<TestModuleDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureTestModule();
    }
}
