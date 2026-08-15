using System;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Sozlesme degisikligi olayinin hangi kosullarda kosum tetikledigini dogrular.
// sistemdeki gorevi: Breaking olmayan veya yeni bulgu tasimayan olaylarin sessiz kalmasini garanti eder (§2.2).
public class ContractChangeImpactManagerTests
{
    // Tamamlanmis, yeni bulgulu ve breaking olay is uretmelidir.
    [Fact]
    public void A_completed_breaking_run_with_new_findings_should_be_actionable()
    {
        ContractChangeImpactManager.IsActionable(CreateSignal(
            CheckRunStatusCodes.Completed,
            newFindingCount: 3,
            DifferenceSeverityCodes.Breaking)).ShouldBeTrue();
    }

    // Yeni bulgu yoksa hicbir sey yapilmamalidir.
    [Fact]
    public void A_run_without_new_findings_should_not_be_actionable()
    {
        ContractChangeImpactManager.IsActionable(CreateSignal(
            CheckRunStatusCodes.Completed,
            newFindingCount: 0,
            DifferenceSeverityCodes.Breaking)).ShouldBeFalse();
    }

    // En agir siddet breaking degilse hicbir sey yapilmamalidir.
    [Theory]
    [InlineData(DifferenceSeverityCodes.NonBreaking)]
    [InlineData(DifferenceSeverityCodes.DocsOnly)]
    [InlineData(null)]
    public void A_run_without_breaking_severity_should_not_be_actionable(string? severityCode)
    {
        ContractChangeImpactManager.IsActionable(CreateSignal(
            CheckRunStatusCodes.Completed,
            newFindingCount: 5,
            severityCode)).ShouldBeFalse();
    }

    // Terminal olmayan gecisler kosum tetiklememelidir.
    [Theory]
    [InlineData(CheckRunStatusCodes.Pending)]
    [InlineData(CheckRunStatusCodes.Running)]
    [InlineData(CheckRunStatusCodes.Failed)]
    public void A_non_completed_run_should_not_be_actionable(string statusCode)
    {
        ContractChangeImpactManager.IsActionable(CreateSignal(
            statusCode,
            newFindingCount: 5,
            DifferenceSeverityCodes.Breaking)).ShouldBeFalse();
    }

    // Ayni kontrol kosusu her zaman ayni tetikleyici referansini uretmelidir.
    [Fact]
    public void Trigger_reference_should_be_stable_per_check_run()
    {
        var checkRunId = Guid.NewGuid();

        ContractChangeImpactManager.CreateTriggerRef(checkRunId)
            .ShouldBe(ContractChangeImpactManager.CreateTriggerRef(checkRunId));
    }

    // Checker olayinin modul ici karar girdisini kurar.
    private static ContractChangeSignal CreateSignal(
        string statusCode,
        int newFindingCount,
        string? maxSeverityCode)
    {
        return new ContractChangeSignal
        {
            CheckRunId = Guid.NewGuid(),
            StatusCode = statusCode,
            NewFindingCount = newFindingCount,
            MaxSeverityCode = maxSeverityCode
        };
    }
}
