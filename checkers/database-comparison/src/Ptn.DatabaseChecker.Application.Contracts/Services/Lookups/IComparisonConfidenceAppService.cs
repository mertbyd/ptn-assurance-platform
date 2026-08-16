using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Fark guveni (Exact/Canonical/Approximate/Incomparable) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface IComparisonConfidenceAppService : ILookupAppService<ComparisonConfidenceDto, CreateComparisonConfidenceDto, UpdateComparisonConfidenceDto>
{
}
