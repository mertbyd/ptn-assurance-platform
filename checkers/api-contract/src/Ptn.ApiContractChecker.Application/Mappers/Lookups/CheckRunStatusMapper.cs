using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Lookups;

// islevi: CheckRunStatus entity, DTO ve lookup model donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Run durumu yonetimi akisinda elle alan kopyalamayi engeller.
[Mapper]
public partial class CheckRunStatusMapper
{
    // Durum entity'sini dis sozlesme DTO'suna donusturur.
    public partial CheckRunStatusDto MapToDto(CheckRunStatus entity);
    // Durum entity listesini dis sozlesme DTO listesine donusturur.
    public partial List<CheckRunStatusDto> MapToDto(List<CheckRunStatus> entities);
    // Create girdisini domain create modeline donusturur.
    public partial LookupCreateModel MapToCreateModel(CreateCheckRunStatusDto dto);
    // Update girdisini invariant-guvenli domain update modeline donusturur.
    public partial LookupUpdateModel MapToUpdateModel(UpdateCheckRunStatusDto dto);
}
