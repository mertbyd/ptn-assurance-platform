using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Karsilastirma modu (SchemaOnly/DataOnly/Both) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface IComparisonTypeAppService : ILookupAppService<ComparisonTypeDto, CreateComparisonTypeDto, UpdateComparisonTypeDto>
{
}
