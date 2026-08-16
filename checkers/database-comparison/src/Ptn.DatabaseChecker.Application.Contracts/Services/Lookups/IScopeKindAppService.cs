using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Kapsam kurali turu (Include/Exclude/DataCompare) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface IScopeKindAppService : ILookupAppService<ScopeKindDto, CreateScopeKindDto, UpdateScopeKindDto>
{
}
