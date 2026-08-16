using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Lookups;

// islevi: DifferenceKind entity, DTO ve lookup model donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Kapali fark katalogu yonetimi akisinda elle alan kopyalamayi engeller.
[Mapper]
public partial class DifferenceKindMapper
{
    // Fark turu entity'sini dis sozlesme DTO'suna donusturur.
    public partial DifferenceKindDto MapToDto(DifferenceKind entity);
    // Fark turu entity listesini dis sozlesme DTO listesine donusturur.
    public partial List<DifferenceKindDto> MapToDto(List<DifferenceKind> entities);
    // Create girdisini domain create modeline donusturur.
    public partial LookupCreateModel MapToCreateModel(CreateDifferenceKindDto dto);
    // Update girdisini invariant-guvenli domain update modeline donusturur.
    public partial LookupUpdateModel MapToUpdateModel(UpdateDifferenceKindDto dto);
}
