using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Data;
using Ptn.TestModule.EntityFrameworkCore;
using Ptn.TestModule.Services.Lookups;
using Shouldly;
using Volo.Abp.Data;
using Xunit;

namespace Ptn.TestModule.Lookups;

// islevi: Bes lookup'in dis okuma yuzeyinin seed edilen kodlari dondurdugunu ve yazma ucu tasimadigini dogrular.
// sistemdeki gorevi: Ajanin lookup kodlarini kaynak okumadan kesfetmesini korur (RULE-0007) ve seed sahipligini yazmaya acilmaya karsi savunur.
public class LookupSurfaceTests : TestModuleEntityFrameworkCoreTestBase
{
    // Yazma fiili tasimamasi gereken lookup controller dosyalaridir.
    private static readonly string[] LookupControllerFileNames =
    [
        "TestRunStatusController.cs",
        "TestOutcomeStatusController.cs",
        "TestFailureCategoryController.cs",
        "TestTriggerKindController.cs",
        "TestScenarioStateController.cs"
    ];

    // Salt-okunur yuzeyde bulunmamasi gereken HTTP fiil attribute'lari.
    private static readonly string[] ForbiddenVerbs = ["[HttpPost", "[HttpPut", "[HttpDelete", "[HttpPatch"];

    // Bes okuma ucu de seed edilen kod kumesinin tamamini dis sozlesmeden dondurmelidir.
    [Fact]
    public async Task Should_expose_every_seeded_lookup_code_through_the_read_surface()
    {
        await RunSeedAsync();

        var exposedCodes = await ReadAllCodesFromAppServicesAsync();

        exposedCodes[nameof(TestRunStatusCodes)].ShouldBe(Sorted(TestRunStatusCodes.All));
        exposedCodes[nameof(TestOutcomeStatusCodes)].ShouldBe(Sorted(TestOutcomeStatusCodes.All));
        exposedCodes[nameof(TestFailureCategoryCodes)].ShouldBe(Sorted(TestFailureCategoryCodes.All));
        exposedCodes[nameof(TestTriggerKindCodes)].ShouldBe(Sorted(TestTriggerKindCodes.All));
        exposedCodes[nameof(TestScenarioStateCodes)].ShouldBe(Sorted(TestScenarioStateCodes.All));
    }

    // Test hukmu ucu build politikasini de tasimalidir; ajan bunu koddan cikarmak zorunda kalmaz.
    [Fact]
    public async Task Should_expose_the_build_breaking_policy_on_the_outcome_surface()
    {
        await RunSeedAsync();

        var page = await WithUnitOfWorkAsync(() =>
            ServiceProvider.GetRequiredService<ITestOutcomeStatusAppService>().GetListAsync(CreateListInput()));

        page.Items.ShouldContain(item => item.Code == TestOutcomeStatusCodes.Failed && item.BreaksBuild);
        page.Items.ShouldContain(item => item.Code == TestOutcomeStatusCodes.Passed && !item.BreaksBuild);
    }

    // Lookup satirlari DataSeedContributor ile gelir; hicbir controller yazma fiili tanimlamamalidir.
    [Fact]
    public void Should_not_expose_any_write_endpoint_on_the_lookup_controllers()
    {
        var controllerDirectory = Path.Combine(
            FindModuleRoot().FullName,
            "src",
            "Ptn.TestModule.HttpApi",
            "Controllers",
            "Lookups");

        foreach (var fileName in LookupControllerFileNames)
        {
            var source = File.ReadAllText(Path.Combine(controllerDirectory, fileName));

            source.ShouldContain("[HttpGet");
            foreach (var verb in ForbiddenVerbs)
            {
                source.ShouldNotContain(verb);
            }
        }
    }

    // Seed katkicisini gercek bir is biriminde kosar.
    private Task RunSeedAsync()
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var contributor = ServiceProvider.GetRequiredService<TestModuleLookupDataSeedContributor>();
            await contributor.SeedAsync(new DataSeedContext());
        });
    }

    // Bes dis okuma sozlesmesinin dondurdugu kod kumelerini tek okumada toplar.
    private Task<Dictionary<string, IReadOnlyList<string>>> ReadAllCodesFromAppServicesAsync()
    {
        return WithUnitOfWorkAsync(async () => new Dictionary<string, IReadOnlyList<string>>
        {
            [nameof(TestRunStatusCodes)] = Sorted((await ServiceProvider
                .GetRequiredService<ITestRunStatusAppService>().GetListAsync(CreateListInput())).Items.Select(item => item.Code)),
            [nameof(TestOutcomeStatusCodes)] = Sorted((await ServiceProvider
                .GetRequiredService<ITestOutcomeStatusAppService>().GetListAsync(CreateListInput())).Items.Select(item => item.Code)),
            [nameof(TestFailureCategoryCodes)] = Sorted((await ServiceProvider
                .GetRequiredService<ITestFailureCategoryAppService>().GetListAsync(CreateListInput())).Items.Select(item => item.Code)),
            [nameof(TestTriggerKindCodes)] = Sorted((await ServiceProvider
                .GetRequiredService<ITestTriggerKindAppService>().GetListAsync(CreateListInput())).Items.Select(item => item.Code)),
            [nameof(TestScenarioStateCodes)] = Sorted((await ServiceProvider
                .GetRequiredService<ITestScenarioStateAppService>().GetListAsync(CreateListInput())).Items.Select(item => item.Code))
        });
    }

    // Tum seed satirlarinin tek sayfada gelmesini garanti eden liste girdisini kurar.
    private static LookupListInput CreateListInput()
    {
        return new LookupListInput { MaxResultCount = 100 };
    }

    private static IReadOnlyList<string> Sorted(IEnumerable<string> codes)
    {
        return codes.OrderBy(code => code, StringComparer.Ordinal).ToList();
    }

    // Test assembly konumundan cozum sahibi modul kokunu bulur.
    private static DirectoryInfo FindModuleRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ptn.TestModule.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("Ptn.TestModule.slnx bulunamadi.");
    }
}
