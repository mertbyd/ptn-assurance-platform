using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Lookups;

// islevi: DifferenceDirection entity, DTO ve lookup model donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Fark yonu yonetimi akisinda elle alan kopyalamayi engeller.
[Mapper]
public partial class DifferenceDirectionMapper
{
    // Yon entity'sini dis sozlesme DTO'suna donusturur.
    public partial DifferenceDirectionDto MapToDto(DifferenceDirection entity);
    // Yon entity listesini dis sozlesme DTO listesine donusturur.
    public partial List<DifferenceDirectionDto> MapToDto(List<DifferenceDirection> entities);
    // Create girdisini domain create modeline donusturur.
    public partial LookupCreateModel MapToCreateModel(CreateDifferenceDirectionDto dto);
    // Update girdisini invariant-guvenli domain update modeline donusturur.
    public partial LookupUpdateModel MapToUpdateModel(UpdateDifferenceDirectionDto dto);
}
