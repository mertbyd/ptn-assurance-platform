using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Application.Mappers.Definitions;
using Ptn.DatabaseChecker.Application.Services;
using Ptn.DatabaseChecker.Dtos.Definitions;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Interface.Definitions;
using Ptn.DatabaseChecker.Managers.Definitions;
using Ptn.DatabaseChecker.Models.Definitions;
using Ptn.DatabaseChecker.Services.Definitions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Application.Services.Definitions;

// islevi: Karsilastirma tanimlari icin temel CRUD Application akisini yonetir.
// sistemdeki gorevi: Tarif ad benzersizligi ve FK kurallari manager'da kalirken servis DTO/model/entity akisini birlestirir.
[RemoteService(IsEnabled = false)]
public class ComparisonDefinitionAppService
    : EntityCrudAppServiceBase<ComparisonDefinition, ComparisonDefinition, ComparisonDefinitionDto, CreateComparisonDefinitionDto, UpdateComparisonDefinitionDto, CreateComparisonDefinitionModel, UpdateComparisonDefinitionModel>,
        IComparisonDefinitionAppService
{
    // Mapperly source-generated mapper; stateless oldugu icin tek statik ornek yeterli.
    private static readonly ComparisonDefinitionMapper Mapper = new();

    // Tarif domain kurallari manager katmaninda isletilir.
    private ComparisonDefinitionManager Manager => LazyGetRequiredService<ComparisonDefinitionManager>();

    // Tarif detay okumalari connection/type Include ile repository katmaninda yapilir.
    private IComparisonDefinitionRepository DefinitionRepository => LazyGetRequiredService<IComparisonDefinitionRepository>();

    public ComparisonDefinitionAppService(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IComparisonDefinitionRepository repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // islevi: Tek tarifi detaylariyla repository'den okur.
    protected override async Task<ComparisonDefinition> GetReadModelAsync(Guid id)
        => EnsureFound(await DefinitionRepository.FindWithDetailsAsync(id), id);

    // Mutation da liste/get ile ayni tenant + host-kullanici gorunurluk sinirindan gecmelidir.
    protected override async Task<ComparisonDefinition> GetEntityForMutationAsync(Guid id)
        => EnsureFound(await DefinitionRepository.FindWithDetailsAsync(id), id);

    // islevi: Tarifleri detaylariyla sayfali okur.
    protected override Task<List<ComparisonDefinition>> GetPagedReadModelsAsync(PagedResultRequestDto input)
        => DefinitionRepository.GetPagedWithDetailsAsync(input.SkipCount, input.MaxResultCount);

    // islevi: Kayit sonrasi tarifleri detaylariyla tek batch sorguda okur.
    protected override Task<List<ComparisonDefinition>> GetReadModelsByIdsAsync(List<Guid> ids)
        => DefinitionRepository.GetWithDetailsByIdsAsync(ids);

    protected override Task<long> GetTotalCountAsync(PagedResultRequestDto input)
        => DefinitionRepository.GetAccessibleCountAsync();

    // islevi: Tarif entity'sini API cevap DTO'suna tasir.
    protected override ComparisonDefinitionDto MapToDto(ComparisonDefinition readModel) => Mapper.MapToDto(readModel);

    // islevi: Tarif entity listesini API cevap DTO listesine tasir.
    protected override List<ComparisonDefinitionDto> MapToDto(List<ComparisonDefinition> readModels) => Mapper.MapToDto(readModels);

    // islevi: Tarif create DTO'sunu domain manager modeline tasir.
    protected override CreateComparisonDefinitionModel MapToCreateModel(CreateComparisonDefinitionDto input) => Mapper.MapToCreateModel(input);

    // islevi: Tarif update DTO'sunu domain manager modeline tasir.
    protected override UpdateComparisonDefinitionModel MapToUpdateModel(UpdateComparisonDefinitionDto input) => Mapper.MapToUpdateModel(input);

    // islevi: Tarif create modelini manager kurallarindan gecirir.
    protected override Task<CreateComparisonDefinitionModel> ValidateCreateModelAsync(CreateComparisonDefinitionModel model)
        => Manager.ValidateCreateAsync(model);

    // islevi: Tarif create model listesini manager batch kurallarindan gecirir.
    protected override Task<List<CreateComparisonDefinitionModel>> ValidateCreateModelsAsync(List<CreateComparisonDefinitionModel> models)
        => Manager.ValidateCreateManyAsync(models);

    // islevi: Tarif update modelini manager kurallarindan gecirir.
    protected override Task<UpdateComparisonDefinitionModel> ValidateUpdateModelAsync(ComparisonDefinition entity, UpdateComparisonDefinitionModel model)
        => Manager.ValidateUpdateAsync(entity, model);

    // islevi: Yeni ComparisonDefinition entity'sini ABP GuidGenerator kimligiyle kurar.
    protected override ComparisonDefinition CreateEmptyEntity() => new(GuidGenerator.Create());

    // islevi: Dogrulanmis tarif create modelini entity uzerine uygular.
    protected override void MapToEntity(CreateComparisonDefinitionModel model, ComparisonDefinition entity) => Mapper.MapToEntity(model, entity);

    // islevi: Dogrulanmis tarif update modelini entity uzerine uygular.
    protected override void MapToEntity(UpdateComparisonDefinitionModel model, ComparisonDefinition entity) => Mapper.MapToEntity(model, entity);
}
