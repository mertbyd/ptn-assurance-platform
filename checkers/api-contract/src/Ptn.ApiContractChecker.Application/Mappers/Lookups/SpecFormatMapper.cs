using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Lookups;

// islevi: SpecFormat entity, DTO ve lookup model donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Format yonetimi akisinda elle alan kopyalamayi engeller.
[Mapper]
public partial class SpecFormatMapper
{
    // Format entity'sini dis sozlesme DTO'suna donusturur.
    public partial SpecFormatDto MapToDto(SpecFormat entity);
    // Format entity listesini dis sozlesme DTO listesine donusturur.
    public partial List<SpecFormatDto> MapToDto(List<SpecFormat> entities);
    // Create girdisini domain create modeline donusturur.
    public partial LookupCreateModel MapToCreateModel(CreateSpecFormatDto dto);
    // Update girdisini invariant-guvenli domain update modeline donusturur.
    public partial LookupUpdateModel MapToUpdateModel(UpdateSpecFormatDto dto);
}
