using Ptn.DatabaseChecker.Dtos.Lookups;

namespace Ptn.DatabaseChecker.Services.Lookups;

// islevi: Sema nesne turu (Table/View/Column/...) lookup CRUD kontratini tanimlar.
// sistemdeki gorevi: 5'li CRUD imzalari generic ILookupAppService'ten gelir; bu arayuz yalnizca DTO tiplerini baglar.
public interface ISchemaObjectTypeAppService : ILookupAppService<SchemaObjectTypeDto, CreateSchemaObjectTypeDto, UpdateSchemaObjectTypeDto>
{
}
