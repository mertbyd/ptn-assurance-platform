using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Ptn.ApiContractChecker.Runs;

// islevi: ContractCheckRun yasam dongusu, owned bulgu ve denormalize sayac invariantlarini dogrular.
// sistemdeki gorevi: Asenkron job tekrar tesliminin terminal sonucu bozmasini ve sayaclarin bulgulardan ayrismasini engeller.
public class ContractCheckRun_Tests
{
    // Terminal geciste sayaclari owned bulgulardan bir kez hesaplar ve ikinci completion'i no-op yapar.
    [Fact]
    public void Complete_Should_Calculate_Counts_And_Be_Idempotent()
    {
        var pendingId = Guid.NewGuid();
        var runningId = Guid.NewGuid();
        var completedId = Guid.NewGuid();
        var failedId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var run = new ContractCheckRun(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), pendingId, Guid.NewGuid());
        var manager = CreateManager();
        manager.Start(run, runningId, startedAt);
        var findings = new ContractCheckFindings(
        [
            CreateFinding(DifferenceSeverityCodes.Breaking),
            CreateFinding(DifferenceSeverityCodes.NonBreaking),
            CreateFinding(DifferenceSeverityCodes.DocsOnly)
        ]);

        var completed = manager.Complete(run, completedId, startedAt.AddSeconds(1), findings);
        var repeated = manager.Fail(run, failedId, startedAt.AddSeconds(2), "late retry");

        completed.ShouldBeTrue();
        repeated.ShouldBeFalse();
        run.CheckRunStatusId.ShouldBe(completedId);
        run.BreakingCount.ShouldBe(1);
        run.NonBreakingCount.ShouldBe(1);
        run.DocsOnlyCount.ShouldBe(1);
        run.ErrorMessage.ShouldBeNull();
    }

    // Pending run'in Running olmadan terminal duruma gecmesini reddeder.
    [Fact]
    public void Complete_Should_Reject_A_Run_That_Has_Not_Started()
    {
        var run = new ContractCheckRun(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var manager = CreateManager();

        var exception = Should.Throw<BusinessException>(() =>
            manager.Complete(run, Guid.NewGuid(), DateTime.UtcNow, ContractCheckFindings.Empty()));

        exception.Code.ShouldBe(ContractCheckRunExceptionCodes.InvalidStatusTransition);
    }

    // Eksik bulgu govdesinin kararli is hatasi ve alan metadata'si tasidigini kanitlar.
    [Fact]
    public void Complete_Should_Reject_Missing_Findings_With_A_Stable_Code()
    {
        var run = new ContractCheckRun(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var manager = CreateManager();
        manager.Start(run, Guid.NewGuid(), DateTime.UtcNow);

        var exception = Should.Throw<BusinessException>(() =>
            manager.Complete(run, Guid.NewGuid(), DateTime.UtcNow, null!));

        exception.Code.ShouldBe(ContractCheckRunExceptionCodes.FindingsRequired);
        exception.Data[BusinessExceptionDataKeys.Field].ShouldBe("findings");
    }

    // Manager'in Failed ve Partial terminal gecislerini entity veri kabuguna eksiksiz uyguladigini kanitlar.
    [Fact]
    public void Lifecycle_Transitions_Should_Apply_Failed_And_Partial_Statuses()
    {
        var tenantId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var failedRun = new ContractCheckRun(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            tenantId);
        var partialRun = new ContractCheckRun(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            tenantId);
        var manager = CreateManager();
        var failedStatusId = Guid.NewGuid();
        var partialStatusId = Guid.NewGuid();

        manager.Start(failedRun, Guid.NewGuid(), startedAt);
        manager.Fail(
            failedRun,
            failedStatusId,
            startedAt.AddSeconds(1),
            ContractCheckRunExceptionCodes.ExecutionFailed);
        manager.Start(partialRun, Guid.NewGuid(), startedAt);
        manager.CompletePartially(
            partialRun,
            partialStatusId,
            startedAt.AddSeconds(1),
            ContractCheckFindings.Empty(),
            ContractCheckRunExceptionCodes.ExecutionFailed);

        failedRun.CheckRunStatusId.ShouldBe(failedStatusId);
        failedRun.ErrorMessage.ShouldBe(ContractCheckRunExceptionCodes.ExecutionFailed);
        partialRun.CheckRunStatusId.ShouldBe(partialStatusId);
        partialRun.ErrorMessage.ShouldBe(ContractCheckRunExceptionCodes.ExecutionFailed);
    }

    // Tamamen bos bulgu adresinin rapora girmesini reddeder.
    [Fact]
    public void FindingAddress_Should_Require_At_Least_One_Component()
    {
        var exception = Should.Throw<BusinessException>(() => new FindingAddress());

        exception.Code.ShouldBe(ContractCheckRunExceptionCodes.FindingAddressRequired);
    }

    // Tek severity ile gecerli owned bulgu kurar.
    private static Finding CreateFinding(string severityCode)
    {
        return new Finding(
            DifferenceKindCodes.DescriptionChanged,
            severityCode,
            DifferenceDirectionCodes.Documentation,
            new FindingAddress(path: "/orders"));
    }

    // Saf gecis testlerinde persistence ve lazy servisleri kullanmadan manager'i kurar.
    private static ContractCheckRunManager CreateManager()
    {
        return new ContractCheckRunManager(
            Substitute.For<IContractCheckRunRepository>(),
            Substitute.For<IAbpLazyServiceProvider>());
    }
}
