using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Lookups;

// islevi: DifferenceSeverity entity, DTO ve lookup model donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Fark siddeti yonetimi akisinda elle alan kopyalamayi engeller.
[Mapper]
public partial class DifferenceSeverityMapper
{
    // Siddet entity'sini dis sozlesme DTO'suna donusturur.
    public partial DifferenceSeverityDto MapToDto(DifferenceSeverity entity);
    // Siddet entity listesini dis sozlesme DTO listesine donusturur.
    public partial List<DifferenceSeverityDto> MapToDto(List<DifferenceSeverity> entities);
    // Create girdisini domain create modeline donusturur.
    public partial LookupCreateModel MapToCreateModel(CreateDifferenceSeverityDto dto);
    // Update girdisini invariant-guvenli domain update modeline donusturur.
    public partial LookupUpdateModel MapToUpdateModel(UpdateDifferenceSeverityDto dto);
}
