using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.DatabaseChecker.BackgroundJobs.Runs;
using Ptn.DatabaseChecker.Application.Mappers.Reports;
using Ptn.DatabaseChecker.Application.Mappers.Runs;
using Ptn.DatabaseChecker.Application.Services;
using Ptn.DatabaseChecker.Dtos.Reports;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.Dtos.Findings;
using Ptn.DatabaseChecker.Dtos.Scopes;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Runs;
using Ptn.DatabaseChecker.Managers.Reports;
using Ptn.DatabaseChecker.Managers.Runs;
using Ptn.DatabaseChecker.Models.Runs;
using Ptn.DatabaseChecker.Services.Runs;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.BackgroundJobs;

namespace Ptn.DatabaseChecker.Application.Services.Runs;

// islevi: Karsilastirma run kayitlari icin okuma, agir detay ve create Application akisini yonetir.
// sistemdeki gorevi: Liste/get header'i owned jsonb cekmeyen projeksiyonla okur (perf); detay ise tam entity'yi findings/kapsam/rapor ile getirir. Run tarihcesi append-only tutulur.
[RemoteService(IsEnabled = false)]
public class ComparisonRunAppService
    : EntityCreateAppServiceBase<ComparisonRun, ComparisonRunHeaderModel, ComparisonRunDto, CreateComparisonRunDto, CreateComparisonRunModel>,
        IComparisonRunAppService
{
    // Mapperly source-generated mapper'lar; stateless olduklari icin tek statik ornek yeterli.
    private static readonly ComparisonRunMapper Mapper = new();
    private static readonly ComparisonReportMapper ReportMapper = new();

    // Run domain kurallari manager katmaninda isletilir.
    private ComparisonRunManager Manager => LazyGetRequiredService<ComparisonRunManager>();

    // Bir tariften Pending run snapshot modelini hazirlayan domain servisidir; agir motor I/O'su background job'da calisir.
    private ComparisonRunExecutionManager ExecutionManager => LazyGetRequiredService<ComparisonRunExecutionManager>();

    // Bulgulardan ozet + tur/tablo gruplama ureten saf rapor servisi.
    private ComparisonReportBuilder ReportBuilder => LazyGetRequiredService<ComparisonReportBuilder>();

    // Run header projeksiyonu ve tam-detay okumasi tipli repository uzerinden yapilir.
    private IComparisonRunRepository RunRepository => LazyGetRequiredService<IComparisonRunRepository>();

    // Calistir-ve-sakla istegi format validasyonu FluentValidation ile Application.Contracts katmanindan gelir.
    private IValidator<ExecuteComparisonRunDto> ExecuteValidator => LazyGetRequiredService<IValidator<ExecuteComparisonRunDto>>();

    // Filtreli owned JSON bulgu sorgusunu sayfa ve cikti butcesiyle calistirir.
    private FindingQueryManager FindingQueryManager => LazyGetRequiredService<FindingQueryManager>();

    // Pending run execution isini ABP retry kuyruguna tasir.
    private readonly IBackgroundJobManager _backgroundJobManager;

    public ComparisonRunAppService(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IComparisonRunRepository repository,
        IBackgroundJobManager backgroundJobManager)
        : base(abpLazyServiceProvider, repository)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    // islevi: Run'i tum bulgular ve rapor icerikleriyle (agir owned veri) getirir.
    public async Task<ComparisonRunDetailDto> GetDetailAsync(Guid id)
    {
        var entity = await RunRepository.FindWithDetailsAsync(id);
        if (entity == null)
        {
            throw new BusinessException(GeneralExceptionCodes.NotFound);
        }

        return Mapper.MapToDetailDto(entity);
    }

    // islevi: Bir tarif icin Pending run kaydi olusturur ve agir comparison execution isini background job'a kuyruklar.
    // sistemdeki gorevi: HTTP yalniz dogrulama -> Pending snapshot -> insert -> enqueue -> detay akisini bekler; Running/Completed/Failed gecisleri worker'da kalici tutulur.
    public async Task<ComparisonRunDetailDto> ExecuteAsync(ExecuteComparisonRunDto input)
    {
        await ExecuteValidator.ValidateAndThrowAsync(input);

        var result = await ExecutionManager.PrepareAsync(input.ComparisonDefinitionId);
        var entity = CreateEmptyEntity();
        Mapper.MapExecutionResultToEntity(result, entity);

        var inserted = await Repository.InsertAsync(entity, autoSave: true);
        await _backgroundJobManager.EnqueueAsync(new ComparisonRunExecutionJobArgs
        {
            ComparisonRunId = inserted.Id,
            TenantId = CurrentTenant.Id,
            ScopeRules = input.ScopeRules ?? new List<ScopeRuleDto>()
        });
        var detail = await RunRepository.FindWithDetailsAsync(inserted.Id);
        if (detail == null)
        {
            throw new BusinessException(GeneralExceptionCodes.NotFound);
        }

        return Mapper.MapToDetailDto(detail);
    }

    // islevi: Run'in yapisal raporunu (ozet + tur/tablo gruplama + detay findings) uretir.
    // sistemdeki gorevi: Header/detay eslemesi tamamen Mapperly'de (entity -> DTO, navigation duzlestirme + findings ic ice); saf ComparisonReportBuilder yalnizca bulgulardan aggregation (ozet + gruplar) hesaplar ve DTO'ya iliştirilir. Rapor calistirma aninda saklanmaz, bulgulardan uretilir.
    public async Task<ComparisonReportDto> GetReportAsync(Guid id)
    {
        var entity = await RunRepository.FindWithDetailsAsync(id);
        if (entity == null)
        {
            throw new BusinessException(GeneralExceptionCodes.NotFound);
        }

        var dto = ReportMapper.MapToDto(entity);
        var aggregation = ReportBuilder.Build(entity.Findings);
        dto.Summary = ReportMapper.MapToDto(aggregation.Summary);
        dto.ObjectTypeGroups = ReportMapper.MapToDto(aggregation.ObjectTypeGroups);
        dto.TableGroups = ReportMapper.MapToDto(aggregation.TableGroups);
        return dto;
    }

    /// <summary>
    /// Bulgu sorgu DTO'sunu modele tasir ve butcelenmis sayfayi ABP sonucuna cevirir.
    /// </summary>
    public async Task<PagedResultDto<FindingDto>> GetFindingsAsync(
        Guid id,
        FindingQueryInput input,
        CancellationToken cancellationToken = default)
    {
        var query = Mapper.MapToQueryModel(input);
        var page = await FindingQueryManager.GetFindingsAsync(id, query, cancellationToken);
        var items = Mapper.MapToFindingDtos(page.Items);
        return new PagedResultDto<FindingDto>(page.TotalCount, items);
    }

    // islevi: Tek run'in hafif header projeksiyonunu repository'den okur (owned jsonb cekmez).
    protected override async Task<ComparisonRunHeaderModel> GetReadModelAsync(Guid id)
        => EnsureFound(await RunRepository.FindHeaderAsync(id), id);

    // islevi: Run header'larini en yeniden eskiye sayfali okur (owned jsonb cekmez).
    protected override Task<List<ComparisonRunHeaderModel>> GetPagedReadModelsAsync(PagedResultRequestDto input)
        => RunRepository.GetPagedHeadersAsync(null, input.SkipCount, input.MaxResultCount);

    // islevi: Kayit sonrasi run header'larini tek batch sorguda okur.
    protected override Task<List<ComparisonRunHeaderModel>> GetReadModelsByIdsAsync(List<Guid> ids)
        => RunRepository.GetHeadersByIdsAsync(ids);

    // islevi: Filtre olmadan run toplam sayisini repository'den alir.
    protected override Task<long> GetTotalCountAsync(PagedResultRequestDto input)
        => RunRepository.GetCountByDefinitionAsync(null);

    // islevi: Run header projeksiyonunu API cevap DTO'suna tasir.
    protected override ComparisonRunDto MapToDto(ComparisonRunHeaderModel readModel) => Mapper.MapToDto(readModel);

    // islevi: Run header projeksiyon listesini API cevap DTO listesine tasir.
    protected override List<ComparisonRunDto> MapToDto(List<ComparisonRunHeaderModel> readModels) => Mapper.MapToDto(readModels);

    // islevi: Run create DTO'sunu domain manager modeline tasir.
    protected override CreateComparisonRunModel MapToCreateModel(CreateComparisonRunDto input) => Mapper.MapToCreateModel(input);

    // islevi: Run create modelini manager kurallarindan gecirir.
    protected override Task<CreateComparisonRunModel> ValidateCreateModelAsync(CreateComparisonRunModel model)
        => Manager.ValidateCreateAsync(model);

    // islevi: Run create model listesini manager batch kurallarindan gecirir.
    protected override Task<List<CreateComparisonRunModel>> ValidateCreateModelsAsync(List<CreateComparisonRunModel> models)
        => Manager.ValidateCreateManyAsync(models);

    // islevi: Yeni ComparisonRun entity'sini ABP GuidGenerator kimligiyle kurar.
    protected override ComparisonRun CreateEmptyEntity() => new(GuidGenerator.Create());

    // islevi: Dogrulanmis run create modelini entity uzerine uygular.
    protected override void MapToEntity(CreateComparisonRunModel model, ComparisonRun entity) => Mapper.MapToEntity(model, entity);
}
