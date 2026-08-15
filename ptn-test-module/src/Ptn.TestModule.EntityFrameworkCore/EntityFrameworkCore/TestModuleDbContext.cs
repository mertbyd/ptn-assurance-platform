using Microsoft.EntityFrameworkCore;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore;

// islevi: Test Module'un tek DbContext'idir; senaryo, kosum ve is bilgisi aggregate koklerini tasir.
// sistemdeki gorevi: Checker ve Auth tablolarina FK vermeden yalniz kendi semalarinin modelini kurar.
/// <summary>
/// Test Module'un sahip oldugu lookup, katalog ve kosum entity'lerinin EF Core context'idir.
/// </summary>
[ConnectionStringName(TestModuleDbProperties.ConnectionStringName)]
public class TestModuleDbContext : AbpDbContext<TestModuleDbContext>, ITestModuleDbContext
{
    /// <summary>Test senaryosu surumlerinin DbSet'idir.</summary>
    public DbSet<TestScenario> TestScenarios { get; set; } = null!;

    // Sabit deger listeleri (test_lookup semasi); global referans verisidir, IMultiTenant tasimaz (ADR-0016 §D).
    /// <summary>Kosum motoru durum lookup kayitlarinin DbSet'idir.</summary>
    public DbSet<TestRunStatus> TestRunStatuses { get; set; } = null!;

    /// <summary>Test hukum lookup kayitlarinin DbSet'idir.</summary>
    public DbSet<TestOutcomeStatus> TestOutcomeStatuses { get; set; } = null!;

    /// <summary>Hata kategorisi lookup kayitlarinin DbSet'idir.</summary>
    public DbSet<TestFailureCategory> TestFailureCategories { get; set; } = null!;

    /// <summary>Tetikleyici turu lookup kayitlarinin DbSet'idir.</summary>
    public DbSet<TestTriggerKind> TestTriggerKinds { get; set; } = null!;

    /// <summary>Senaryo durumu lookup kayitlarinin DbSet'idir.</summary>
    public DbSet<TestScenarioState> TestScenarioStates { get; set; } = null!;

    /// <summary>Test kosum aggregate kayitlarinin DbSet'idir.</summary>
    public DbSet<TestRun> TestRuns { get; set; } = null!;

    /// <summary>Test kosum sonucu aggregate kayitlarinin DbSet'idir.</summary>
    public DbSet<TestRunResult> TestRunResults { get; set; } = null!;

    /// <summary>Test sonucu bulgu cocuk kayitlarinin DbSet'idir.</summary>
    public DbSet<TestResultFinding> TestResultFindings { get; set; } = null!;

    /// <summary>Context'i provider secenekleriyle kurar.</summary>
    public TestModuleDbContext(DbContextOptions<TestModuleDbContext> options)
        : base(options)
    {
    }

    /// <summary>Test Module entity configuration'larini EF modeline uygular.</summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureTestModule();
    }
}
