using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Rapor formati (Html/Markdown) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface IReportFormatAppService : ILookupAppService<ReportFormatDto, CreateReportFormatDto, UpdateReportFormatDto>
{
}
