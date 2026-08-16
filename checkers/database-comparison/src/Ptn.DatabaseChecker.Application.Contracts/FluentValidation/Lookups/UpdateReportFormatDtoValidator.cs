using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.FluentValidation.Lookups;

// islevi: Rapor formati (Html/Markdown) update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Tum kurallar ortak LookupUpdateDtoValidator tabanindan gelir; bu sinif yalnizca DTO tipini baglar.
public class UpdateReportFormatDtoValidator : LookupUpdateDtoValidator<UpdateReportFormatDto>
{
}
