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

// islevi: Sema nesne turu (Table/View/Column/...) lookup'inin CRUD akisini yonetir.
// sistemdeki gorevi: Tum akis ve benzersizlik kurali LookupAppServiceBase + LookupManager'dan gelir; burada yalnizca entity kurulumu ve Mapperly baglantilari vardir.
[RemoteService(IsEnabled = false)]
public class SchemaObjectTypeAppService
    : LookupAppServiceBase<SchemaObjectType, SchemaObjectTypeDto, CreateSchemaObjectTypeDto, UpdateSchemaObjectTypeDto>, ISchemaObjectTypeAppService
{
    // Mapperly source-generated mapper; stateless oldugu icin tek statik ornek yeterli.
    private static readonly SchemaObjectTypeMapper Mapper = new();

    public SchemaObjectTypeAppService(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<SchemaObjectType, Guid> repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // Dogrulanmis modelden yeni entity kurar; kimlik ABP GuidGenerator'dan gelir, alanlar entity ctor'una devredilir.
    protected override SchemaObjectType CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Entity -> DTO donusumu Mapperly'ye devredilir.
    protected override SchemaObjectTypeDto MapToDto(SchemaObjectType entity) => Mapper.MapToDto(entity);

    // Liste donusumu Mapperly'ye devredilir.
    protected override List<SchemaObjectTypeDto> MapToDto(List<SchemaObjectType> entities) => Mapper.MapToDto(entities);

    // Create DTO -> domain model donusumu Mapperly'ye devredilir.
    protected override LookupCreateModel MapToCreateModel(CreateSchemaObjectTypeDto input) => Mapper.MapToCreateModel(input);

    // Update DTO -> domain model donusumu Mapperly'ye devredilir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateSchemaObjectTypeDto input) => Mapper.MapToUpdateModel(input);

    // Dogrulanmis update modelini mevcut entity uzerine Mapperly ile yazar.
    protected override void ApplyUpdate(LookupUpdateModel model, SchemaObjectType entity) => Mapper.MapToEntity(model, entity);
}
