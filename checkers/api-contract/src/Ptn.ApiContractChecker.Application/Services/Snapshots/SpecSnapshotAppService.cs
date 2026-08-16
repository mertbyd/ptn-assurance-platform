using Ptn.ApiContractChecker.Application.Mappers.Snapshots;
using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Models.Snapshots;
using Ptn.ApiContractChecker.Services.Snapshots;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Managers.Snapshots;
using FluentValidation;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.Diagnostics;
using Ptn.ApiContractChecker.Constants.Diagnostics;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Application.Services.Snapshots;

// islevi: Bir dokumanin snapshot gecmisi ve tekil anlik goruntu okuma senaryolarini orkestre eder.
// sistemdeki gorevi: Dokuman sahipligini manager'a, sorgulari repository'ye birakir; detay tam grafigi, liste ham govdesiz basligi doner.
[RemoteService(IsEnabled = false)]
public class SpecSnapshotAppService
    : EntityReadAppServiceBase<
        SpecSnapshot,
        SpecSnapshot,
        SpecSnapshotHeaderModel,
        SpecSnapshotDetailDto,
        SpecSnapshotHeaderDto,
        GetSpecSnapshotsInput>,
      ISpecSnapshotAppService
{
    private static readonly SpecSnapshotMapper Mapper = new();

    // Kaynak gorunurlugu ve dokuman sahipligi kurallarini domain katmaninda isletir.
    private SpecSourceManager SourceManager => LazyGetRequiredService<SpecSourceManager>();

    // Gecmis sayfalamasini ve navigation'li detay okumasini persistence katmaninda calistirir.
    private ISpecSnapshotRepository SnapshotRepository => LazyGetRequiredService<ISpecSnapshotRepository>();

    // Operasyon/sema ozet, verbosity, butce ve resultRef kurallarini domain katmaninda isletir.
    private SpecSnapshotAuthoringManager AuthoringManager => LazyGetRequiredService<SpecSnapshotAuthoringManager>();

    // Operasyon envanteri sorgusunun kapali kod ve sayfa penceresi kurallarini calistirir.
    private IValidator<ListSnapshotOperationsInput> InventoryValidator =>
        LazyGetRequiredService<IValidator<ListSnapshotOperationsInput>>();

    // Ortak operasyon secimi public input'unu dogrular.
    private IValidator<OperationSelectionDto> OperationValidator =>
        LazyGetRequiredService<IValidator<OperationSelectionDto>>();

    // Sema describe public input'unu dogrular.
    private IValidator<DescribeSchemaDto> SchemaValidator => LazyGetRequiredService<IValidator<DescribeSchemaDto>>();

    public SpecSnapshotAppService(
        IAbpLazyServiceProvider provider,
        ISpecSnapshotRepository repository)
        : base(provider, repository)
    {
    }

    // Dokuman sahipligini bir kez dogrular, sayma ve sayfalama akisini tabana birakir.
    public override async Task<PagedResultDto<SpecSnapshotHeaderDto>> GetListAsync(GetSpecSnapshotsInput input)
    {
        await SourceManager.GetRequiredActiveDocumentAsync(input.SpecSourceId, input.SpecDocumentId);
        return await base.GetListAsync(input);
    }

    // Snapshot'i yukler, envanter sorgusunu dogrular ve butceli operasyon sayfasini Mapperly ile tasir.
    public async Task<SnapshotOperationInventoryDto> ListOperationsAsync(
        Guid snapshotId,
        ListSnapshotOperationsInput input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.SnapshotDescribeSpan,
            ApiContractCheckerDiagnostics.MomentAuthoring);
        await InventoryValidator.ValidateAndThrowAsync(input);
        var snapshot = await SnapshotRepository.FindWithDetailsAsync(snapshotId);
        var request = Mapper.MapToInventoryRequest(input);
        var result = await AuthoringManager.ListOperationsAsync(snapshot, request);
        var response = Mapper.MapToInventoryDto(result);
        ApiContractCheckerActivity.SetResponseBytes(activity, response.ResponseBytes);
        return response;
    }

    // Snapshot'i yukler, operasyon secimini dogrular ve butceli ozeti Mapperly ile tasir.
    public async Task<OperationSummaryDto> FindOperationAsync(OperationSelectionDto input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.SnapshotDescribeSpan,
            ApiContractCheckerDiagnostics.MomentAuthoring);
        await OperationValidator.ValidateAndThrowAsync(input);
        var snapshot = await SnapshotRepository.FindWithDetailsAsync(input.SnapshotId);
        var selection = Mapper.MapToSelection(input);
        var result = await AuthoringManager.FindOperationAsync(snapshot, selection, input.VerbosityCode);
        var response = Mapper.MapToDto(result);
        ApiContractCheckerActivity.SetResponseBytes(activity, result.MeasureUtf8Bytes());
        return response;
    }

    // Snapshot'i yukler, sema ref'ini dogrular ve tek-seviye ozeti Mapperly ile tasir.
    public async Task<SchemaDescriptionDto> DescribeSchemaAsync(DescribeSchemaDto input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.SnapshotDescribeSpan,
            ApiContractCheckerDiagnostics.MomentAuthoring);
        await SchemaValidator.ValidateAndThrowAsync(input);
        var snapshot = await SnapshotRepository.FindWithDetailsAsync(input.SnapshotId);
        var result = await AuthoringManager.DescribeSchemaAsync(snapshot, input.SchemaRef, input.VerbosityCode);
        var response = Mapper.MapToDto(result);
        ApiContractCheckerActivity.SetResponseBytes(activity, result.MeasureUtf8Bytes());
        return response;
    }

    // ResultRef'in cagirici bagini yeniden denetleyip tam ozeti yeniden calistirmadan getirir.
    public Task<SnapshotAuthoringResultDto> GetAuthoringResultAsync(string resultRef)
    {
        var result = AuthoringManager.FindResult(resultRef)
                     ?? throw new BusinessException(GeneralExceptionCodes.NotFound);
        return Task.FromResult(Mapper.MapToDto(result));
    }

    // Tek snapshot'i bagli icerik ve format satirlariyla birlikte okur.
    protected override async Task<SpecSnapshot> GetReadModelAsync(Guid id)
    {
        return EnsureFound(await SnapshotRepository.FindWithDetailsAsync(id), id);
    }

    // Dokumanin gecmisini ham govde tasimayan basliklar halinde sayfalar.
    protected override Task<List<SpecSnapshotHeaderModel>> GetPagedReadModelsAsync(GetSpecSnapshotsInput input)
    {
        return SnapshotRepository.GetPagedHeadersForDocumentAsync(
            input.SpecDocumentId,
            input.SkipCount,
            input.MaxResultCount);
    }

    // Liste toplam sayisini satirlarla ayni dokuman filtresinde hesaplar.
    protected override Task<long> GetTotalCountAsync(GetSpecSnapshotsInput input)
    {
        return SnapshotRepository.GetHeaderCountForDocumentAsync(input.SpecDocumentId);
    }

    // Yuklenen snapshot grafigini detay DTO'suna tasir.
    protected override SpecSnapshotDetailDto MapToDetailDto(SpecSnapshot readModel)
    {
        return Mapper.MapToDetailDto(readModel);
    }

    // Hafif gecmis satirlarini liste DTO'larina tasir.
    protected override List<SpecSnapshotHeaderDto> MapToListDto(List<SpecSnapshotHeaderModel> readModels)
    {
        return Mapper.MapToHeaderDto(readModels);
    }
}
