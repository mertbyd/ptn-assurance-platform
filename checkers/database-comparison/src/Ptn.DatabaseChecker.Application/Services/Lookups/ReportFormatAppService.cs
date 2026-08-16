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

// islevi: Rapor formati (Html/Markdown) lookup'inin CRUD akisini yonetir.
// sistemdeki gorevi: Tum akis ve benzersizlik kurali LookupAppServiceBase + LookupManager'dan gelir; burada yalnizca entity kurulumu ve Mapperly baglantilari vardir.
[RemoteService(IsEnabled = false)]
public class ReportFormatAppService
    : LookupAppServiceBase<ReportFormat, ReportFormatDto, CreateReportFormatDto, UpdateReportFormatDto>, IReportFormatAppService
{
    // Mapperly source-generated mapper; stateless oldugu icin tek statik ornek yeterli.
    private static readonly ReportFormatMapper Mapper = new();

    public ReportFormatAppService(
        IAbpLazyServiceProvider abpLazyServiceProvider,
        IRepository<ReportFormat, Guid> repository)
        : base(abpLazyServiceProvider, repository)
    {
    }

    // Dogrulanmis modelden yeni entity kurar; kimlik ABP GuidGenerator'dan gelir, alanlar entity ctor'una devredilir.
    protected override ReportFormat CreateEntity(LookupCreateModel model)
        => new(GuidGenerator.Create(), model.Code, model.Name, model.Description, model.IsActive);

    // Entity -> DTO donusumu Mapperly'ye devredilir.
    protected override ReportFormatDto MapToDto(ReportFormat entity) => Mapper.MapToDto(entity);

    // Liste donusumu Mapperly'ye devredilir.
    protected override List<ReportFormatDto> MapToDto(List<ReportFormat> entities) => Mapper.MapToDto(entities);

    // Create DTO -> domain model donusumu Mapperly'ye devredilir.
    protected override LookupCreateModel MapToCreateModel(CreateReportFormatDto input) => Mapper.MapToCreateModel(input);

    // Update DTO -> domain model donusumu Mapperly'ye devredilir.
    protected override LookupUpdateModel MapToUpdateModel(UpdateReportFormatDto input) => Mapper.MapToUpdateModel(input);

    // Dogrulanmis update modelini mevcut entity uzerine Mapperly ile yazar.
    protected override void ApplyUpdate(LookupUpdateModel model, ReportFormat entity) => Mapper.MapToEntity(model, entity);
}
