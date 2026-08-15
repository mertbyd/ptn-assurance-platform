using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Runs;

namespace Ptn.TestModule.EntityFrameworkCore;

// islevi: Test Module DbContext'inin katmanlar arasi kullanilabilir DbSet sozlesmesini tanimlar.
// sistemdeki gorevi: Varsayilan ABP repository kaydini yalniz modulun sahip oldugu entity'lerle sinirlar.
/// <summary>
/// Test Module'un EF Core DbSet yuzeyini tanimlar.
/// </summary>
[ConnectionStringName(TestModuleDbProperties.ConnectionStringName)]
public interface ITestModuleDbContext : IEfCoreDbContext
{
    /// <summary>Test senaryosu surumlerinin DbSet'idir.</summary>
    DbSet<TestScenario> TestScenarios { get; }

    /// <summary>Test kosum aggregate kayitlarinin DbSet'idir.</summary>
    DbSet<TestRun> TestRuns { get; }

    /// <summary>Test kosum sonucu aggregate kayitlarinin DbSet'idir.</summary>
    DbSet<TestRunResult> TestRunResults { get; }

    /// <summary>Test sonucu bulgu cocuk kayitlarinin DbSet'idir.</summary>
    DbSet<TestResultFinding> TestResultFindings { get; }
}
