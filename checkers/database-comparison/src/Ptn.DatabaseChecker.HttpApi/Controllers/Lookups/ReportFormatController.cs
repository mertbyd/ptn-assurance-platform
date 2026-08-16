using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Services.Lookups;

namespace Ptn.DatabaseChecker.Controllers.Lookups;

// islevi: Rapor formati (Html/Markdown) lookup CRUD endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: Tum endpoint govdeleri LookupControllerBase'ten gelir; bu sinif yalnizca rotayi ve servis tipini baglar.
/// <summary>
/// Rapor formati (Html/Markdown) lookup yonetimi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.ReportFormats)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Lookups)]
public class ReportFormatController
    : LookupControllerBase<IReportFormatAppService, ReportFormatDto, CreateReportFormatDto, UpdateReportFormatDto>
{
}
