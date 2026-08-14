using System.Collections.Generic;
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

// islevi: Lookup seed'inin tekrar kosumda satir cogaltmadigini ve kod kumesinin sabitlerle ortusdugunu dogrular.
// sistemdeki gorevi: Seed'in idempotansi bozulursa FK cozumlemesi ikiye bolunur; bu testler o regresyonu yakalar.
public class LookupSeedTests : TestModuleEntityFrameworkCoreTestBase
{
    // Seed'i iki kez kosar; ikinci kosum sonrasi kod kumeleri birinciyle ayni kalmalidir.
    [Fact]
    public async Task Should_not_duplicate_rows_when_seeded_twice()
    {
        await RunSeedAsync();
        var afterFirstRun = await GetAllCodesAsync();

        await RunSeedAsync();
        var afterSecondRun = await GetAllCodesAsync();

        afterSecondRun.ShouldBe(afterFirstRun);
    }

    // Bes lookup'in seed edilen kod kumesi ilgili *Codes.All ile birebir eslesmelidir.
    [Fact]
    public async Task Should_seed_exactly_the_shared_constant_code_sets()
    {
        await RunSeedAsync();

        var seededCodes = await GetAllCodesAsync();

        seededCodes[nameof(TestRunStatusCodes)].ShouldBe(Sorted(TestRunStatusCodes.All));
        seededCodes[nameof(TestOutcomeStatusCodes)].ShouldBe(Sorted(TestOutcomeStatusCodes.All));
        seededCodes[nameof(TestFailureCategoryCodes)].ShouldBe(Sorted(TestFailureCategoryCodes.All));
        seededCodes[nameof(TestTriggerKindCodes)].ShouldBe(Sorted(TestTriggerKindCodes.All));
        seededCodes[nameof(TestScenarioStateCodes)].ShouldBe(Sorted(TestScenarioStateCodes.All));
    }

    // Seed katkicisini gercek bir is biriminde kosar; testler kasitli olarak ikinci kez cagirabilsin diye acikca tetiklenir.
    private Task RunSeedAsync()
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var contributor = ServiceProvider.GetRequiredService<TestModuleLookupDataSeedContributor>();
            await contributor.SeedAsync(new DataSeedContext());
        });
    }

    // Bes tablonun kod kumesini tek okumada, karsilastirilabilir sirayla toplar.
    private Task<Dictionary<string, IReadOnlyList<string>>> GetAllCodesAsync()
    {
        return WithUnitOfWorkAsync(async () => new Dictionary<string, IReadOnlyList<string>>
        {
            [nameof(TestRunStatusCodes)] =
                Sorted((await ServiceProvider.GetRequiredService<ITestRunStatusRepository>().GetListAsync()).Select(row => row.Code)),
            [nameof(TestOutcomeStatusCodes)] =
                Sorted((await ServiceProvider.GetRequiredService<ITestOutcomeStatusRepository>().GetListAsync()).Select(row => row.Code)),
            [nameof(TestFailureCategoryCodes)] =
                Sorted((await ServiceProvider.GetRequiredService<ITestFailureCategoryRepository>().GetListAsync()).Select(row => row.Code)),
            [nameof(TestTriggerKindCodes)] =
                Sorted((await ServiceProvider.GetRequiredService<ITestTriggerKindRepository>().GetListAsync()).Select(row => row.Code)),
            [nameof(TestScenarioStateCodes)] =
                Sorted((await ServiceProvider.GetRequiredService<ITestScenarioStateRepository>().GetListAsync()).Select(row => row.Code))
        });
    }

    private static IReadOnlyList<string> Sorted(IEnumerable<string> codes)
    {
        return codes.OrderBy(code => code, System.StringComparer.Ordinal).ToList();
    }
}
