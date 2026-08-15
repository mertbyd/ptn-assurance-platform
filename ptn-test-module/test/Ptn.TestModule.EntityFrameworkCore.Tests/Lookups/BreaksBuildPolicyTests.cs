using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Data;
using Ptn.TestModule.EntityFrameworkCore;
using Ptn.TestModule.Interface.Lookups;
using Shouldly;
using Volo.Abp.Data;
using Xunit;

namespace Ptn.TestModule.Lookups;

// islevi: Build kirma politikasinin dogru hukumlerde tasindigini dogrular.
// sistemdeki gorevi: Politika koda degil breaks_build kolonuna baglidir; yanlis seed edilirse basarisiz kosum sessizce yesil gorunur (ADR-0016 §F).
public class BreaksBuildPolicyTests : TestModuleEntityFrameworkCoreTestBase
{
    // Yalniz Failed ve Broken build'i kirar; kalan uc hukum kirmaz.
    [Theory]
    [InlineData(TestOutcomeStatusCodes.Failed, true)]
    [InlineData(TestOutcomeStatusCodes.Broken, true)]
    [InlineData(TestOutcomeStatusCodes.Passed, false)]
    [InlineData(TestOutcomeStatusCodes.Skipped, false)]
    [InlineData(TestOutcomeStatusCodes.Inconclusive, false)]
    public async Task Should_carry_the_expected_build_policy(string code, bool expectedBreaksBuild)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var contributor = ServiceProvider.GetRequiredService<TestModuleLookupDataSeedContributor>();
            await contributor.SeedAsync(new DataSeedContext());
        });

        var breaksBuild = await WithUnitOfWorkAsync(async () =>
        {
            var rows = await ServiceProvider.GetRequiredService<ITestOutcomeStatusRepository>().GetListAsync();
            return rows.Single(row => row.Code == code).BreaksBuild;
        });

        breaksBuild.ShouldBe(expectedBreaksBuild);
    }
}
