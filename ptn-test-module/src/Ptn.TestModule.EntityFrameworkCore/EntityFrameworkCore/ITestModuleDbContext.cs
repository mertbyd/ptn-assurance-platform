using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore;

[ConnectionStringName(TestModuleDbProperties.ConnectionStringName)]
public interface ITestModuleDbContext : IEfCoreDbContext
{
    Microsoft.EntityFrameworkCore.DbSet<Ptn.TestModule.Entities.Catalog.TestScenario> TestScenarios { get; }
}
