using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.Services.Lookups;

// islevi: DifferenceSeverity lookup yonetimi servis kontratini tanimlar.
// sistemdeki gorevi: Ortak lookup operasyonlarini fark siddeti DTO tipleriyle controller'a acar.
public interface IDifferenceSeverityAppService : ILookupAppService<DifferenceSeverityDto, CreateDifferenceSeverityDto, UpdateDifferenceSeverityDto>
{
}
