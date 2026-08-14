using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore;

[ConnectionStringName(TestModuleDbProperties.ConnectionStringName)]
public interface ITestModuleDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
