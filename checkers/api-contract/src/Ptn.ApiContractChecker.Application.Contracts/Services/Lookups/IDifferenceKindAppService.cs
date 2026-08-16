using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.Services.Lookups;

// islevi: DifferenceKind lookup yonetimi servis kontratini tanimlar.
// sistemdeki gorevi: Ortak lookup operasyonlarini kapali fark katalogu DTO tipleriyle controller'a acar.
public interface IDifferenceKindAppService : ILookupAppService<DifferenceKindDto, CreateDifferenceKindDto, UpdateDifferenceKindDto>
{
}
