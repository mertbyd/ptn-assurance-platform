using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using NSubstitute;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Ptn.ApiContractChecker.Runs;

// islevi: ContractCheckReportBuilder deterministik gruplama, bos ozet ve entity sayac tutarliligini dogrular.
// sistemdeki gorevi: Kalici olmayan rapor hesaplamasinin EF ve HTTP katmanlarindan bagimsiz domain kanitidir.
public class ContractCheckReportBuilder_Tests
{
    private readonly ContractCheckReportBuilder _builder = new();

    // Ayni bulgular farkli girdi sirasinda gelse de ayni raporu ve run sayaclariyla tutarli ozeti uretir.
    [Fact]
    public void Build_Should_Be_Deterministic_And_Match_Run_Counts()
    {
        var firstFinding = CreateFinding(DifferenceSeverityCodes.Breaking, "/z-orders");
        var secondFinding = CreateFinding(DifferenceSeverityCodes.NonBreaking, "/a-orders");
        var thirdFinding = CreateFinding(DifferenceSeverityCodes.DocsOnly, "/m-orders");
        var findings = new ContractCheckFindings([firstFinding, secondFinding, thirdFinding]);
        var reorderedFindings = new ContractCheckFindings([thirdFinding, firstFinding, secondFinding]);
        var run = new ContractCheckRun(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        var manager = new ContractCheckRunManager(
            Substitute.For<IContractCheckRunRepository>(),
            Substitute.For<IAbpLazyServiceProvider>());
        var startedAt = DateTime.UtcNow;
        manager.Start(run, Guid.NewGuid(), startedAt);
        manager.Complete(run, Guid.NewGuid(), startedAt.AddSeconds(1), findings);

        var first = _builder.Build(findings);
        var second = _builder.Build(reorderedFindings);

        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
        first.Summary.BreakingCount.ShouldBe(run.BreakingCount);
        first.Summary.NonBreakingCount.ShouldBe(run.NonBreakingCount);
        first.Summary.DocsOnlyCount.ShouldBe(run.DocsOnlyCount);
        first.Groups.Single().Findings.Select(finding => finding.Address.Path)
            .ShouldBe(["/a-orders", "/m-orders", "/z-orders"]);
    }

    // Bos findings govdesinin sifir sayacli ve bos kirilimli gecerli rapor urettigini kanitlar.
    [Fact]
    public void Build_Should_Return_Empty_Summary_For_Empty_Findings()
    {
        var report = _builder.Build(ContractCheckFindings.Empty());

        report.Summary.TotalFindingCount.ShouldBe(0);
        report.Summary.BreakingCount.ShouldBe(0);
        report.Summary.NonBreakingCount.ShouldBe(0);
        report.Summary.DocsOnlyCount.ShouldBe(0);
        report.Summary.SeverityCounts.ShouldBeEmpty();
        report.Summary.DirectionCounts.ShouldBeEmpty();
        report.Summary.KindCounts.ShouldBeEmpty();
        report.Groups.ShouldBeEmpty();
    }

    // Ayni kind grubunda farkli severity ve adres tasiyan gecerli bulgu kurar.
    private static Finding CreateFinding(string severityCode, string path)
    {
        return new Finding(
            DifferenceKindCodes.DescriptionChanged,
            severityCode,
            DifferenceDirectionCodes.Documentation,
            new FindingAddress(path: path));
    }
}
