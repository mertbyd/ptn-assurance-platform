using System.Text;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Models.Runs;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Runs;

// islevi: Contract-check run'ini hazirlar, donmus baglamini kurar, motoru calistirir ve idempotent terminal sonucu uygular.
// sistemdeki gorevi: HTTP ile background job arasindaki Pending -> Running -> terminal yasam dongusunu kisa UOW sinirlariyla tek domain kapisinda toplar.
public class ContractCheckRunExecutionManager : ApiContractCheckerDomainService
{
    // Aggregate kurulum, claim, terminal gecis ve stale recovery kurallarinin sahibi.
    private ContractCheckRunManager RunManager => LazyGetRequiredService<ContractCheckRunManager>();

    // Execution projection ve run persistence'i tek repository zincirinde tutar.
    private IContractCheckRunRepository RunRepository => LazyGetRequiredService<IContractCheckRunRepository>();

    // Donmus ham spec govdesini provider-bagimsiz modele ayristirir.
    private ISpecDocumentReader DocumentReader => LazyGetRequiredService<ISpecDocumentReader>();

    // Normalize, scope ve diff adimlarini saf findings sonucuna indirger.
    private SpecComparisonExecutionManager ComparisonManager =>
        LazyGetRequiredService<SpecComparisonExecutionManager>();

    // Tenant-aware deger saklama setting'ini fail-closed politikaya cevirir.
    private ValueRetentionPolicyResolver RetentionPolicyResolver =>
        LazyGetRequiredService<ValueRetentionPolicyResolver>();

    // Saf comparison bulgularini kalicilik sinirinda secilen retention politikasina indirger.
    private FindingValueRetentionManager RetentionManager =>
        LazyGetRequiredService<FindingValueRetentionManager>();

    public ContractCheckRunExecutionManager(IAbpLazyServiceProvider provider)
        : base(provider)
    {
    }

    // Iki mevcut tenant-gorunur snapshot'i dogrular, stale run'lari toparlar ve Pending run'i yazar.
    public async Task<ContractCheckRun> PrepareAsync(Guid baseSnapshotId, Guid targetSnapshotId)
    {
        await RunManager.RecoverStaleRunningAsync(
            Clock.Now.AddMinutes(-ContractCheckRunConsts.StaleRunningThresholdMinutes));
        var run = await RunManager.CreateAsync(
            GuidGenerator.Create(),
            baseSnapshotId,
            targetSnapshotId,
            CurrentTenant.Id);
        return await RunRepository.InsertAsync(run, autoSave: true);
    }

    // Pending run'i tek worker'a claim eder; tekrar veya terminal teslimde no-op bildirir.
    public Task<bool> StartAsync(Guid runId)
    {
        return RunManager.StartAsync(runId);
    }

    // Run'in iki degismez snapshot govdesini kisa UOW icinde repositorysiz execution baglamina tasir.
    public async Task<ContractCheckExecutionContextModel> BuildContextAsync(
        Guid runId,
        List<ContractCheckScopeRuleModel> scopeRules,
        bool ignoreInternal)
    {
        var snapshots = await RunRepository.FindExecutionSnapshotPairAsync(runId)
                        ?? throw new BusinessException(GeneralExceptionCodes.NotFound);

        return new ContractCheckExecutionContextModel
        {
            RunId = runId,
            BaseContent = Encoding.UTF8.GetBytes(snapshots.BaseContent),
            TargetContent = Encoding.UTF8.GetBytes(snapshots.TargetContent),
            ScopeRules = scopeRules,
            IgnoreInternal = ignoreInternal
        };
    }

    // Donmus baglamdaki iki spec'i UOW olmadan ayristirip normalize eder ve karsilastirir.
    public async Task<ContractCheckExecutionResultModel> ExecuteAsync(
        ContractCheckExecutionContextModel context)
    {
        var baseSpec = await DocumentReader.ReadAsync(context.BaseContent);
        var targetSpec = await DocumentReader.ReadAsync(context.TargetContent);
        var findings = ComparisonManager.Compare(
            baseSpec.Snapshot,
            targetSpec.Snapshot,
            context.ScopeRules,
            context.IgnoreInternal);
        var retentionPolicy = await RetentionPolicyResolver.ResolveAsync();

        return new ContractCheckExecutionResultModel
        {
            RunId = context.RunId,
            Findings = RetentionManager.Apply(findings, retentionPolicy)
        };
    }

    // Basarili sonucu idempotent entity terminal gecisiyle tek yazimda tamamlar; gecis gercekten olduysa true doner.
    public Task<bool> CompleteAsync(ContractCheckExecutionResultModel result)
    {
        return RunManager.CompleteAsync(result.RunId, result.Findings);
    }

    // Basarisiz execution'i kararli guvenli sebep koduyla idempotent terminal duruma getirir.
    public Task FailAsync(Guid runId, string errorCode)
    {
        return RunManager.FailAsync(runId, errorCode);
    }
}
