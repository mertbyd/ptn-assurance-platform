using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Fark yonu (OnlyInSource/OnlyInTarget/Modified) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface IDifferenceKindAppService : ILookupAppService<DifferenceKindDto, CreateDifferenceKindDto, UpdateDifferenceKindDto>
{
}
