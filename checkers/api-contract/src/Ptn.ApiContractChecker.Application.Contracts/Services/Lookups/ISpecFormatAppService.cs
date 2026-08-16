using Ptn.ApiContractChecker.Dtos.Lookups;

namespace Ptn.ApiContractChecker.Services.Lookups;

// islevi: SpecFormat lookup yonetimi servis kontratini tanimlar.
// sistemdeki gorevi: Ortak lookup operasyonlarini format DTO tipleriyle controller'a acar.
public interface ISpecFormatAppService : ILookupAppService<SpecFormatDto, CreateSpecFormatDto, UpdateSpecFormatDto>
{
}
