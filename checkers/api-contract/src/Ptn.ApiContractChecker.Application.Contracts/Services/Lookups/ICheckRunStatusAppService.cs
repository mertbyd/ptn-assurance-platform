using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.Services.Lookups;

// islevi: CheckRunStatus lookup yonetimi servis kontratini tanimlar.
// sistemdeki gorevi: Ortak lookup operasyonlarini run durumu DTO tipleriyle controller'a acar.
public interface ICheckRunStatusAppService : ILookupAppService<CheckRunStatusDto, CreateCheckRunStatusDto, UpdateCheckRunStatusDto>
{
}
