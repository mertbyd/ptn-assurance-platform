using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Calistirma durumu (Pending/Running/Completed/Failed) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface IComparisonRunStatusAppService : ILookupAppService<ComparisonRunStatusDto, CreateComparisonRunStatusDto, UpdateComparisonRunStatusDto>
{
}
