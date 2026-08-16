using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.Services.Lookups;

// islevi: DifferenceDirection lookup yonetimi servis kontratini tanimlar.
// sistemdeki gorevi: Ortak lookup operasyonlarini fark yonu DTO tipleriyle controller'a acar.
public interface IDifferenceDirectionAppService : ILookupAppService<DifferenceDirectionDto, CreateDifferenceDirectionDto, UpdateDifferenceDirectionDto>
{
}
