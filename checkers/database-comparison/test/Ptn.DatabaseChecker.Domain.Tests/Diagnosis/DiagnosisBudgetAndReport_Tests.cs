using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Localization;
using Ptn.DatabaseChecker.Managers.Diagnosis;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.DatabaseChecker.Diagnosis;

// islevi: Probe adet butcesi, deterministik hipotez sirasi ve 4 KB rapor tavanini dogrular.
// sistemdeki gorevi: Butce asiminda kismi rapor, ayni girdide ayni cikti ve MCP-dostu govde boyutunun regresyon kanitidir.
public class DiagnosisBudgetAndReport_Tests
{
    // islevi: MaxProbeCount asiminda yeni probe baslatmadan kismi evidence dondugunu dogrular.
    [Fact]
    public async Task Probe_Count_Budget_Should_Stop_And_Return_Partial_Evidence()
    {
        var probe = Substitute.For<IDiagnosisProbe>();
        probe.ProbeKindCode.Returns(ProbeKindCodes.RowExists);
        probe.RunAsync(
                Arg.Any<DatabaseConnection>(),
                Arg.Any<ProbeRequest>(),
                Arg.Any<ValueRetentionPolicy>(),
                Arg.Any<System.Threading.CancellationToken>())
            .Returns(call => new ProbeEvidence
            {
                ProbeKindCode = ProbeKindCodes.RowExists,
                HypothesisKindCode = call.Arg<ProbeRequest>().HypothesisKindCode,
                FactCode = ProbeKindCodes.Facts.Missing
            });
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        settings.GetOrNullAsync(DatabaseCheckerSettings.Diagnosis.MaxProbeCount).Returns("1");
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc));
        var manager = new ProbeBudgetManager(settings, clock, new[] { probe });

        var evidence = await manager.RunAsync(
            CreateConnection(),
            new() { Request("A"), Request("B") },
            new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty));

        evidence.Count.ShouldBe(1);
        await probe.Received(1).RunAsync(
            Arg.Any<DatabaseConnection>(),
            Arg.Any<ProbeRequest>(),
            Arg.Any<ValueRetentionPolicy>(),
            Arg.Any<System.Threading.CancellationToken>());
    }

    // islevi: Ayni assessment girdisinin ayni sirali ve serialize edilmis raporu urettigini dogrular.
    [Fact]
    public void Same_Input_Should_Produce_The_Same_Ranked_Report()
    {
        var manager = CreateRankingManager();
        var first = manager.BuildReport(new FailureIdentity(), new ResolvedFailureContext(), Assessments(), 5);
        var second = manager.BuildReport(new FailureIdentity(), new ResolvedFailureContext(), Assessments(), 5);

        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
        first.Hypotheses[0].ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Confirmed);
    }

    // islevi: Sismis kanit ve metinlerin TrimToBudget sonrasinda 4 KB tavani asmadigini dogrular.
    [Fact]
    public void Report_Should_Remain_Under_Four_Kilobytes()
    {
        var report = new DiagnosisReport
        {
            Detail = new string('D', 2000),
            Hypotheses = Enumerable.Range(0, 12).Select(index => new HypothesisAssessment(
                $"H{index:D2}",
                index,
                DiagnosisConfidenceCodes.Possible,
                new()
                {
                    new ProbeEvidence { ObservedValue = new string('V', 1500) },
                    new ProbeEvidence { ObservedValue = new string('W', 1500) }
                })
            {
                Detail = new string('X', 1000),
                NextChecks = new() { new string('N', 500) }
            }).ToList(),
            NextChecks = new() { new string('T', 1000) }
        };

        report.TrimToBudget();

        report.MeasureUtf8Bytes().ShouldBeLessThanOrEqualTo(FailureSourceKindCodes.Report.MaxUtf8Bytes);
        report.Hypotheses.Count.ShouldBeGreaterThan(0);
    }

    // islevi: Probe budget testine tek tur kararli request kurar.
    private static ProbeRequest Request(string hypothesis)
        => new() { ProbeKindCode = ProbeKindCodes.RowExists, HypothesisKindCode = hypothesis };

    // islevi: Probe budget testine PostgreSQL engine navigation'i yuklu baglanti kurar.
    private static DatabaseConnection CreateConnection()
        => new(Guid.NewGuid())
        {
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };

    // islevi: Localization key'ini aynen donduren localizer ile saf ranking manager kurar.
    private static HypothesisRankingManager CreateRankingManager()
    {
        var localizer = Substitute.For<IStringLocalizer<DatabaseCheckerResource>>();
        localizer[Arg.Any<string>()].Returns(call =>
            new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        return new HypothesisRankingManager(localizer);
    }

    // islevi: Siralamada guvenin oncelikten once geldigini gosteren iki assessment kurar.
    private static List<HypothesisAssessment> Assessments()
        => new()
        {
            new HypothesisAssessment("Likely", 100, DiagnosisConfidenceCodes.Likely),
            new HypothesisAssessment("Confirmed", 1, DiagnosisConfidenceCodes.Confirmed)
        };
}
