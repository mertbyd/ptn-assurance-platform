using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: CTRF ve JUnit ihracatinin determinizmini, sayaclarini ve kayipsiz hukum eslemesini dogrular.
// sistemdeki gorevi: Failed ile Broken ayriminin ihracatta duzlestirilmemesini kalici regresyon kapisina cevirir (PLAN-0003 TM-14 §2.2).
public class RunExportManagerTests
{
    // Ayni kosumun iki ayri ihracati bayt-es CTRF uretmelidir.
    [Fact]
    public void Should_produce_byte_identical_ctrf_for_the_same_run()
    {
        var source = CreateSource(TestOutcomeStatusCodes.Failed);

        var first = new CtrfReportManager().Create(source);
        var second = new CtrfReportManager().Create(source);

        Sha256(first).ShouldBe(Sha256(second));
    }

    // Ayni kosumun iki ayri ihracati bayt-es JUnit uretmelidir.
    [Fact]
    public void Should_produce_byte_identical_junit_for_the_same_run()
    {
        var source = CreateSource(TestOutcomeStatusCodes.Broken);

        var first = new JUnitReportManager().Create(source);
        var second = new JUnitReportManager().Create(source);

        Sha256(first).ShouldBe(Sha256(second));
    }

    // Broken CTRF'de other, Failed'de failed olmali; iki hukum ayni kovaya dusmemelidir.
    [Theory]
    [InlineData(TestOutcomeStatusCodes.Passed, CtrfReportConsts.Status.Passed)]
    [InlineData(TestOutcomeStatusCodes.Failed, CtrfReportConsts.Status.Failed)]
    [InlineData(TestOutcomeStatusCodes.Broken, CtrfReportConsts.Status.Other)]
    [InlineData(TestOutcomeStatusCodes.Skipped, CtrfReportConsts.Status.Skipped)]
    [InlineData(TestOutcomeStatusCodes.Inconclusive, CtrfReportConsts.Status.Pending)]
    public void Should_map_every_outcome_to_its_ctrf_status(string outcomeCode, string expected)
    {
        CtrfReportManager.ResolveStatus(outcomeCode).ShouldBe(expected);
    }

    // Broken JUnit'te <error>, Failed'de <failure> olmali; ayrim korunmalidir.
    [Theory]
    [InlineData(TestOutcomeStatusCodes.Failed, JUnitReportConsts.Elements.Failure)]
    [InlineData(TestOutcomeStatusCodes.Broken, JUnitReportConsts.Elements.Error)]
    [InlineData(TestOutcomeStatusCodes.Skipped, JUnitReportConsts.Elements.Skipped)]
    [InlineData(TestOutcomeStatusCodes.Inconclusive, JUnitReportConsts.Elements.Error)]
    public void Should_map_every_outcome_to_its_junit_child_element(string outcomeCode, string expected)
    {
        JUnitReportManager.ResolveChildElement(outcomeCode).ShouldBe(expected);
    }

    // Passed hukmu JUnit'te hicbir cocuk element uretmemelidir.
    [Fact]
    public void Should_not_emit_a_junit_child_for_a_passed_attempt()
    {
        JUnitReportManager.ResolveChildElement(TestOutcomeStatusCodes.Passed).ShouldBeNull();
    }

    // Failed ve Broken ayni belgede farkli elementlere gitmelidir.
    [Fact]
    public void Should_keep_failed_and_broken_apart_in_one_junit_document()
    {
        var source = CreateSource(TestOutcomeStatusCodes.Failed, TestOutcomeStatusCodes.Broken);

        var xml = new JUnitReportManager().Create(source);

        xml.ShouldContain($"<{JUnitReportConsts.Elements.Failure} ");
        xml.ShouldContain($"<{JUnitReportConsts.Elements.Error} ");
        xml.ShouldContain($"{JUnitReportConsts.Attributes.Failures}=\"1\"");
        xml.ShouldContain($"{JUnitReportConsts.Attributes.Errors}=\"1\"");
    }

    // CTRF sayaclari denemelerin hukum dagilimini birebir yansitmalidir.
    [Fact]
    public void Should_count_ctrf_summary_from_the_attempt_outcomes()
    {
        var source = CreateSource(
            TestOutcomeStatusCodes.Passed,
            TestOutcomeStatusCodes.Failed,
            TestOutcomeStatusCodes.Broken);

        var json = new CtrfReportManager().Create(source);

        json.ShouldContain("\"tests\":3");
        json.ShouldContain("\"passed\":1");
        json.ShouldContain("\"failed\":1");
        json.ShouldContain("\"other\":1");
    }

    // Ic hukum kodu CTRF extra alaninda korunmali; kaba durum kayba yol acmamalidir.
    [Fact]
    public void Should_preserve_the_internal_outcome_code_in_ctrf_extra()
    {
        var source = CreateSource(TestOutcomeStatusCodes.Broken);

        var json = new CtrfReportManager().Create(source);

        json.ShouldContain($"\"{CtrfReportConsts.Fields.OutcomeCode}\":\"{TestOutcomeStatusCodes.Broken}\"");
    }

    // Terminal denemesi olmayan kosum ihracata girmeden kararli kodla reddedilmelidir.
    [Fact]
    public void Should_reject_a_run_without_a_terminal_attempt()
    {
        var manager = new RunExportManager(new CtrfReportManager(), new JUnitReportManager());
        var source = new RunExportSource { Run = CreateRun(), Attempts = [] };

        var exception = Should.Throw<BusinessException>(
            () => manager.EnsureExportable(source, source.Run.Id));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.ExportRequiresTerminalResult);
    }

    // Iki format ayri blob adiyla uretilip bag kumesine kayipsiz baglanmalidir.
    [Fact]
    public void Should_create_one_named_artifact_per_format()
    {
        var manager = new RunExportManager(new CtrfReportManager(), new JUnitReportManager());
        var source = CreateSource(TestOutcomeStatusCodes.Passed);

        var artifacts = manager.CreateArtifacts(source, attempt: 1);
        var links = RunExportManager.ToLinks(artifacts);

        artifacts.Count.ShouldBe(2);
        links.CtrfBlobName.ShouldEndWith(RunArtifactConsts.FileNames.Ctrf);
        links.JUnitBlobName.ShouldEndWith(RunArtifactConsts.FileNames.JUnit);
        links.SarifBlobName.ShouldBeNull();
    }

    // Verilen hukumlerden sirali denemeler tasiyan ihracat girdisi kurar.
    private static RunExportSource CreateSource(params string[] outcomeCodes)
    {
        var attempts = new List<RunExportAttempt>();
        foreach (var outcomeCode in outcomeCodes)
        {
            attempts.Add(new RunExportAttempt
            {
                Attempt = attempts.Count + 1,
                OutcomeCode = outcomeCode,
                DurationMs = 1_200,
                ErrorCode = outcomeCode == TestOutcomeStatusCodes.Passed ? null : "STEP_FAILED",
                Detail = outcomeCode == TestOutcomeStatusCodes.Passed ? null : "expected row was absent",
                FailedStepName = outcomeCode == TestOutcomeStatusCodes.Passed ? null : "verify-subject-row",
                FailedStepOrdinal = outcomeCode == TestOutcomeStatusCodes.Passed ? null : 2
            });
        }

        return new RunExportSource { Run = CreateRun(), Attempts = attempts };
    }

    // Ihracat testleri icin kararli alanlari olan bir kosum kabugu kurar.
    private static TestRun CreateRun()
    {
        return new TestRun(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            runStatusId: Guid.Parse("44444444-4444-4444-4444-444444444444"),
            triggerKindId: Guid.Parse("55555555-5555-5555-5555-555555555555"),
            tenantId: null,
            new TestRunCreateModel { TestKey = "orders.create", TriggerKindCode = TestTriggerKindCodes.Manual },
            new TestRunEnvironmentBinding { EnvironmentKey = "staging", BaseUrl = "https://api.test" },
            historyId: new string('a', 64),
            traceId: "0123456789abcdef0123456789abcdef",
            specFingerprint: new string('b', 64),
            dbSchemaFingerprint: new string('c', 64),
            runnerRef: "respect@redocly/cli:2.14.0");
    }

    // Ihracat govdesinin bayt-es olup olmadigini kararli ozetle karsilastirir.
    private static string Sha256(string content)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
    }
}
