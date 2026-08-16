using FluentValidation;
using Ptn.ApiContractChecker.Application.Mappers.Runs;
using Ptn.ApiContractChecker.BackgroundJobs.Runs;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.Dtos.Runs.Reports;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Services.Runs;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Ptn.ApiContractChecker.Diagnostics;
using Ptn.ApiContractChecker.Constants.Diagnostics;

namespace Ptn.ApiContractChecker.Application.Services.Runs;

// islevi: Contract check tetikleme, gecmis, detay, hafif status ve anlik rapor senaryolarini orkestre eder.
// sistemdeki gorevi: Sistem-yazimli Pending run + ABP job akisini acar; okumalarda repository projeksiyonlarini ve domain raporunu Mapperly DTO'larina tasir.
[RemoteService(IsEnabled = false)]
public class ContractCheckRunAppService
    : EntityReadAppServiceBase<
        ContractCheckRun,
        ContractCheckRunDetailModel,
        ContractCheckRunHeaderModel,
        ContractCheckRunDetailDto,
        ContractCheckRunHeaderDto,
        GetContractCheckRunsInput>,
      IContractCheckRunAppService
{
    private static readonly ContractCheckRunMapper Mapper = new();

    // Tenant sahipligi, varlik, durum gecisi ve rapor kurallarini isletir.
    private ContractCheckRunManager Manager => LazyGetRequiredService<ContractCheckRunManager>();

    // Hafif liste ve count projeksiyonlarini persistence katmaninda calistirir.
    private IContractCheckRunRepository RunRepository => LazyGetRequiredService<IContractCheckRunRepository>();

    // Findings govdesinden kalici olmayan deterministik raporu saf olarak uretir.
    private ContractCheckReportBuilder ReportBuilder => LazyGetRequiredService<ContractCheckReportBuilder>();

    // Pending run kurulumunu ve execution yasam dongusunu domain katmaninda sahiplenir.
    private ContractCheckRunExecutionManager ExecutionManager =>
        LazyGetRequiredService<ContractCheckRunExecutionManager>();

    // Tetikleme DTO'sunun sekil kurallarini job kuyrugundan once calistirir.
    private IValidator<ExecuteContractCheckDto> ExecuteValidator =>
        LazyGetRequiredService<IValidator<ExecuteContractCheckDto>>();

    // Bulgu query DTO'sunun kapali kod ve sekil kurallarini repository'den once calistirir.
    private IValidator<GetContractCheckFindingsInput> FindingsValidator =>
        LazyGetRequiredService<IValidator<GetContractCheckFindingsInput>>();

    // Onceki kosu fingerprint kumeleriyle New/Known/Unknown kararini manager katmaninda kurar.
    private FindingChangeStateManager ChangeStateManager =>
        LazyGetRequiredService<FindingChangeStateManager>();

    // Bulgu sayfa adet ve UTF-8 byte esiklerini setting zincirinden cozer.
    private FindingPagePolicyResolver PagePolicyResolver =>
        LazyGetRequiredService<FindingPagePolicyResolver>();

    // Uzun comparison isini ABP'nin kuyruk, retry ve iptal altyapisina devreder.
    private readonly IBackgroundJobManager _backgroundJobManager;

    public ContractCheckRunAppService(
        IAbpLazyServiceProvider provider,
        IContractCheckRunRepository repository,
        IBackgroundJobManager backgroundJobManager)
        : base(provider, repository)
    {
        _backgroundJobManager = backgroundJobManager;
    }
    // Iki mevcut snapshot icin Pending run'i atomik yazip gecici scope ile execution job'unu kuyruklar.
    public virtual async Task<ContractCheckRunStatusDto> ExecuteAsync(ExecuteContractCheckDto input)
    {
        await ExecuteValidator.ValidateAndThrowAsync(input);
        var run = await ExecutionManager.PrepareAsync(input.BaseSnapshotId, input.TargetSnapshotId);
        await _backgroundJobManager.EnqueueAsync(new ContractCheckExecutionJobArgs
        {
            RunId = run.Id,
            TenantId = CurrentTenant.Id,
            ScopeRules = input.ScopeRules ?? [],
            IgnoreInternal = input.IgnoreInternal
        });
        var header = await Manager.GetRequiredHeaderAsync(run.Id);
        return Mapper.MapToStatusDto(header);
    }
    // Findings govdesini cekmeden run status basligini hafif DTO olarak getirir.
    public async Task<ContractCheckRunStatusDto> GetStatusAsync(Guid id)
    {
        var header = await Manager.GetRequiredHeaderAsync(id);
        return Mapper.MapToStatusDto(header);
    }
    // Run findings govdesini yukleyip kalici olmayan deterministik raporu her istekte yeniden uretir.
    public async Task<ContractCheckReportDto> GetReportAsync(Guid id)
    {
        var run = await Manager.GetRequiredDetailAsync(id);
        var report = ReportBuilder.Build(run.Findings);
        return Mapper.MapToReportDto(report);
    }
    // Bulgu govdesini filtreleyip sayfalar, degisim durumunu ekler ve byte butcesini acikca uygular.
    public async Task<FindingPagedResultDto> GetFindingsAsync(
        Guid id,
        GetContractCheckFindingsInput input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.FindingsPageSpan,
            ApiContractCheckerDiagnostics.MomentMaintenance,
            id);
        await FindingsValidator.ValidateAndThrowAsync(input);
        await Manager.GetRequiredHeaderAsync(id);
        var policy = await PagePolicyResolver.ResolveAsync();
        var requestedSize = ResolveRequestedPageSize(input.MaxResultCount, policy.DefaultPageSize);
        var effectiveSize = Math.Min(requestedSize, policy.MaxPageSize);
        var classification = await ChangeStateManager.ClassifyAsync(id, input.SinceRunId);
        var selection = ChangeStateManager.BuildSelection(
            classification, input.ChangeStateCode, input.SinceRunId, input.Fingerprints);
        var totalCount = await GetFindingCountAsync(id, input, selection);
        var findings = await GetFindingPageAsync(id, input, effectiveSize, selection);
        var page = ChangeStateManager.BuildPage(
            findings, classification, totalCount, requestedSize, effectiveSize);
        page.TrimToBudget(policy.MaxResponseBytes);
        var result = Mapper.MapToFindingPageDto(page);
        ApiContractCheckerActivity.SetResponseBytes(activity, result.ResponseBytes);
        return result;
    }
    // Varsayilan DTO degerini tenant-aware setting varsayilanina cevirir.
    private static int ResolveRequestedPageSize(int requestedSize, int defaultSize)
        => requestedSize == Constants.Runs.ContractCheckRunConsts.DefaultFindingPageSize
            ? defaultSize
            : requestedSize;

    // Bulgu count sorgusunu query DTO alanlarini yorumlamadan repository'ye iletir.
    private Task<long> GetFindingCountAsync(
        Guid id,
        GetContractCheckFindingsInput input,
        FindingFingerprintSelectionModel? selection)
        => RunRepository.GetFindingCountAsync(
            id, input.SeverityCode, input.KindCode, input.Path, input.SchemaName, selection);

    // Bulgu sayfa sorgusunu etkili tavanla repository'ye iletir.
    private Task<List<FindingReadModel>> GetFindingPageAsync(
        Guid id,
        GetContractCheckFindingsInput input,
        int effectiveSize,
        FindingFingerprintSelectionModel? selection)
        => RunRepository.GetPagedFindingsAsync(
            id,
            input.SkipCount,
            effectiveSize,
            input.SeverityCode,
            input.KindCode,
            input.Path,
            input.SchemaName,
            selection);
    // Tek run detayini tenant sahipligi ve kararli NotFound davranisiyla getirir.
    protected override Task<ContractCheckRunDetailModel> GetReadModelAsync(Guid id)
    {
        return Manager.GetRequiredDetailAsync(id);
    }

    // Run basliklarini kaynak/dokuman filtreleriyle CreationTime azalan sirada sayfalar.
    protected override Task<List<ContractCheckRunHeaderModel>> GetPagedReadModelsAsync(
        GetContractCheckRunsInput input)
    {
        return RunRepository.GetPagedHeadersAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.SpecDocumentId,
            input.SpecSourceId);
    }

    // Liste toplam sayisini satirlarla ayni kaynak/dokuman filtrelerinde hesaplar.
    protected override Task<long> GetTotalCountAsync(GetContractCheckRunsInput input)
    {
        return RunRepository.GetHeaderCountAsync(input.SpecDocumentId, input.SpecSourceId);
    }

    // Tam run read-model'ini findings dahil detay DTO'suna tasir.
    protected override ContractCheckRunDetailDto MapToDetailDto(ContractCheckRunDetailModel readModel)
    {
        return Mapper.MapToDetailDto(readModel);
    }

    // Hafif run basliklarini liste DTO'larina tasir.
    protected override List<ContractCheckRunHeaderDto> MapToListDto(
        List<ContractCheckRunHeaderModel> readModels)
    {
        return Mapper.MapToHeaderDto(readModels);
    }
}
