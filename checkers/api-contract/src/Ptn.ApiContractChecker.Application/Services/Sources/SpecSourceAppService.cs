using FluentValidation;
using Ptn.ApiContractChecker.Application.Mappers.Sources;
using Ptn.ApiContractChecker.Application.Mappers.Snapshots;
using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Interface.Sources;
using Ptn.ApiContractChecker.Interface.Secrets;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Sources;
using Ptn.ApiContractChecker.Services.Sources;
using Ptn.ApiContractChecker.Secrets;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace Ptn.ApiContractChecker.Application.Services.Sources;

// islevi: SpecSource aggregate CRUD ve pasifleştirme senaryolarını orkestre eder.
// sistemdeki gorevi: Generic CRUD tabanını kullanıp kuralları manager'a, sorguları repository'ye ve eşlemeyi Mapperly'ye bırakır.
[RemoteService(IsEnabled = false)]
public class SpecSourceAppService
    : EntityCrudAppServiceBase<SpecSource, SpecSource, SpecSourceDto, CreateSpecSourceDto, UpdateSpecSourceDto, CreateSpecSourceModel, UpdateSpecSourceModel>,
      ISpecSourceAppService
{
    private static readonly SpecSourceMapper Mapper = new();
    private static readonly SpecSnapshotMapper SnapshotMapper = new();
    private SpecSourceManager Manager => LazyGetRequiredService<SpecSourceManager>();
    private ISpecSourceRepository SourceRepository => LazyGetRequiredService<ISpecSourceRepository>();
    private ISecretProvider SecretProvider => LazyGetRequiredService<ISecretProvider>();
    private SpecIngestionManager IngestionManager => LazyGetRequiredService<SpecIngestionManager>();
    // Izleme istegi taban CRUD sozlesmesinin disinda oldugu icin kendi validator'ini cozer.
    private IValidator<ConfigureSpecDocumentMonitoringDto> MonitoringValidator =>
        LazyGetRequiredService<IValidator<ConfigureSpecDocumentMonitoringDto>>();

    public SpecSourceAppService(IAbpLazyServiceProvider provider, ISpecSourceRepository repository)
        : base(provider, repository)
    {
    }

    public override Task DeleteAsync(Guid id) => Manager.RejectDeleteAsync(id);

    // Kaynagi ve varsa credential secret'ini tek create senaryosunda orkestre eder.
    public override async Task<SpecSourceDto> CreateAsync(CreateSpecSourceDto input)
    {
        await CreateValidator.ValidateAndThrowAsync(input);
        var model = MapToCreateModel(input);
        var source = await CreateEntityAsync(model);
        Manager.SetVaultSecretPath(source, await WriteCredentialAsync(source.Id, null, input));
        return Mapper.MapToDto(await Repository.InsertAsync(source, autoSave: true));
    }

    // Kaynak tanimini yenilerken credential verilmediyse mevcut secret yolunu korur.
    public override async Task<SpecSourceDto> UpdateAsync(Guid id, UpdateSpecSourceDto input)
    {
        await UpdateValidator.ValidateAndThrowAsync(input);
        var source = await GetEntityForMutationAsync(id);
        var model = MapToUpdateModel(input);
        await ValidateUpdateModelAsync(source, model);
        model.VaultSecretPath = await WriteCredentialAsync(source.Id, source.VaultSecretPath, input);
        Manager.Update(source, model);
        return Mapper.MapToDto(await Repository.UpdateAsync(source, autoSave: true));
    }

    public async Task<SpecSourceDto> PassivateAsync(Guid id)
    {
        var source = await GetEntityForMutationAsync(id);
        Manager.Passivate(source);
        return Mapper.MapToDto(await Repository.UpdateAsync(source, autoSave: true));
    }

    // Kaynagin aktif dokumanlarini credential'i aciga cikarmadan dis erisilebilirlikten sinar.
    public async Task<SpecSourceReachabilityDto> TestReachabilityAsync(Guid id)
    {
        var source = await GetEntityForMutationAsync(id);
        return Mapper.MapToReachabilityDto(await Manager.TestReachabilityAsync(source));
    }

    // Uzun dis cekimi UOW disinda tutar ve yalniz snapshot yazimini kisa yerel UOW'de tamamlar.
    [UnitOfWork(IsDisabled = true)]
    public async Task<SpecSnapshotDto> CaptureSnapshotAsync(Guid id, Guid documentId)
    {
        var source = await GetEntityForMutationAsync(id);
        var document = Manager.GetRequiredActiveDocument(source, documentId);
        var fetched = await Manager.FetchDocumentAsync(source, document);
        using var unitOfWork = UnitOfWorkManager.Begin(requiresNew: true);
        var snapshot = await IngestionManager.IngestAsync(document.Id, fetched);
        await unitOfWork.CompleteAsync();
        return SnapshotMapper.MapToDto(snapshot);
    }

    // Dokumanin zamanlanmis izleme tercihini tek kisa mutasyonda uygular; kaynak ad/path yolu bu alanlara dokunmaz.
    public async Task<SpecDocumentMonitoringDto> ConfigureDocumentMonitoringAsync(
        Guid id,
        Guid documentId,
        ConfigureSpecDocumentMonitoringDto input)
    {
        await MonitoringValidator.ValidateAndThrowAsync(input);
        var source = await GetEntityForMutationAsync(id);
        var model = Mapper.MapToMonitoringModel(input);
        var document = Manager.ConfigureDocumentMonitoring(source, documentId, model);
        await Repository.UpdateAsync(source, autoSave: true);
        return Mapper.MapToMonitoringDto(document);
    }

    protected override async Task<SpecSource> GetReadModelAsync(Guid id) =>
        EnsureFound(await SourceRepository.FindWithDetailsAsync(id), id);

    protected override async Task<SpecSource> GetEntityForMutationAsync(Guid id) =>
        EnsureFound(await SourceRepository.FindWithDetailsAsync(id), id);

    protected override Task<List<SpecSource>> GetPagedReadModelsAsync(PagedResultRequestDto input) =>
        SourceRepository.GetPagedWithDetailsAsync(input.SkipCount, input.MaxResultCount);

    protected override Task<List<SpecSource>> GetReadModelsByIdsAsync(List<Guid> ids) =>
        SourceRepository.GetWithDetailsByIdsAsync(ids);

    protected override Task<long> GetTotalCountAsync(PagedResultRequestDto input) =>
        SourceRepository.GetAccessibleCountAsync();

    protected override SpecSourceDto MapToDto(SpecSource readModel) => Mapper.MapToDto(readModel);
    protected override List<SpecSourceDto> MapToDto(List<SpecSource> readModels) => Mapper.MapToDto(readModels);
    protected override CreateSpecSourceModel MapToCreateModel(CreateSpecSourceDto input) => Mapper.MapToCreateModel(input);
    protected override UpdateSpecSourceModel MapToUpdateModel(UpdateSpecSourceDto input) => Mapper.MapToUpdateModel(input);
    protected override Task<CreateSpecSourceModel> ValidateCreateModelAsync(CreateSpecSourceModel model) => Manager.ValidateCreateAsync(model);
    protected override Task<List<CreateSpecSourceModel>> ValidateCreateModelsAsync(List<CreateSpecSourceModel> models) => Manager.ValidateCreateManyAsync(models);
    protected override Task<UpdateSpecSourceModel> ValidateUpdateModelAsync(SpecSource source, UpdateSpecSourceModel model) => Manager.ValidateUpdateAsync(source, model);
    protected override SpecSource BuildEntity(CreateSpecSourceModel model) => Manager.Create(GuidGenerator.Create(), model, CurrentTenant.Id);
    protected override void MapToEntity(UpdateSpecSourceModel model, SpecSource source) => Manager.Update(source, model);

    // Credential cifti varsa Vault'a yazar, yoksa mevcut secret referansini korur.
    private async Task<string?> WriteCredentialAsync(Guid sourceId, string? currentPath, CreateSpecSourceDto input)
    {
        if (string.IsNullOrWhiteSpace(input.HeaderName) || string.IsNullOrWhiteSpace(input.HeaderValue))
        {
            return currentPath;
        }

        var path = currentPath ?? SecretPathBuilder.Build(CurrentTenant.Id, sourceId);
        await SecretProvider.SetAsync(path, Mapper.MapToCredential(input));
        return path;
    }
}
