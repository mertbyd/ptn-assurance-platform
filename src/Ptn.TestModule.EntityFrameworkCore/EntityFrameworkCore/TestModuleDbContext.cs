using Microsoft.EntityFrameworkCore;
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
