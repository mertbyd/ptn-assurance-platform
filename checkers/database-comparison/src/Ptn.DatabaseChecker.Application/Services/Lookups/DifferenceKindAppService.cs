using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Application.Mappers.Lookups;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Models.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Ptn.DatabaseChecker.Application.Services.Lookups;

// islevi: Fark yonu (OnlyInSource/OnlyInTarget/Modified) lookup'inin CRUD akisini yonetir.
// sistemdeki gorevi: Tum akis ve benzersizlik kurali LookupAppServiceBase + LookupManager'dan gelir; burada yalnizca entity kurulumu ve Mapperly baglantilari vardir.
[RemoteService(IsEnabled = false)]
public class DifferenceKindAppService
    : LookupAppServiceBase<DifferenceKind, DifferenceKindDto, CreateDifferenceKindDto, UpdateDifferenceKindDto>, IDifferenceKindAppService
{
    // Mapperly source-generated mapper; stateless oldugu icin tek statik ornek yeterli.
    private static readonly DifferenceKindMapper Mapper = new();

    public DifferenceKindAppService(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<DifferenceKind, Guid> repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // Dogrulanmis modelden yeni entity kurar; kimlik ABP GuidGenerator'dan gelir, alanlar entity ctor'una devredilir.
    protected override DifferenceKind CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Entity -> DTO donusumu Mapperly'ye devredilir.
    protected override DifferenceKindDto MapToDto(DifferenceKind entity) => Mapper.MapToDto(entity);

    // Liste donusumu Mapperly'ye devredilir.
    protected override List<DifferenceKindDto> MapToDto(List<DifferenceKind> entities) => Mapper.MapToDto(entities);

    // Create DTO -> domain model donusumu Mapperly'ye devredilir.
    protected override LookupCreateModel MapToCreateModel(CreateDifferenceKindDto input) => Mapper.MapToCreateModel(input);

    // Update DTO -> domain model donusumu Mapperly'ye devredilir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateDifferenceKindDto input) => Mapper.MapToUpdateModel(input);

    // Dogrulanmis update modelini mevcut entity uzerine Mapperly ile yazar.
    protected override void ApplyUpdate(LookupUpdateModel model, DifferenceKind entity) => Mapper.MapToEntity(model, entity);
}
