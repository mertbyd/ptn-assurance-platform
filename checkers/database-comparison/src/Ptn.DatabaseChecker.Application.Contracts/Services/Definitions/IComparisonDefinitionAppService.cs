using Ptn.DatabaseChecker.Dtos.Definitions;

namespace Ptn.DatabaseChecker.Services.Definitions;

// islevi: Karsilastirma tanimi temel CRUD servis kontratini tanimlar.
// sistemdeki gorevi: Tekrar kullanilabilir comparison tariflerinin Application uzerinden yonetilmesini saglar.
public interface IComparisonDefinitionAppService : IEntityCrudAppService<ComparisonDefinitionDto, CreateComparisonDefinitionDto, UpdateComparisonDefinitionDto>
{
}
