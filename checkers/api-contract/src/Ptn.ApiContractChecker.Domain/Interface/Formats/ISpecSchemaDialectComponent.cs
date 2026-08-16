using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Interface.Formats;

// islevi: Normalize semayi kaynak OpenAPI dialect'ine uygun validator dugumune cevirir.
// sistemdeki gorevi: Format farklarini ISpecSchemaResolver icindeki kosullardan ayirir.
public interface ISpecSchemaDialectComponent : ISpecFormatComponent
{
    // Saf sema modelini NJsonSchema'nin validate edebilecegi dugume cevirir.
    Task<ResolvedSpecSchemaModel> BuildAsync(SpecSchemaModel schema);
}
