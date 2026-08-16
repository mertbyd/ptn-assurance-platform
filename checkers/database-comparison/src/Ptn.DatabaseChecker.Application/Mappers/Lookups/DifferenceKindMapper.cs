using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Lookups;

// islevi: Fark yonu (OnlyInSource/OnlyInTarget/Modified) lookup'inin DTO/model/entity donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Elle property kopyalamayi engeller; tum lookup CRUD akisinin mapping'i bu sinifta toplanir.
[Mapper]
public partial class DifferenceKindMapper
{
    // Entity'yi API cikti DTO'suna cevirir.
    public partial DifferenceKindDto MapToDto(DifferenceKind entity);

    // Listeleme icin entity koleksiyonunu DTO listesine cevirir.
    public partial List<DifferenceKindDto> MapToDto(List<DifferenceKind> entities);

    // Create istegini domain create modeline cevirir.
    public partial LookupCreateModel MapToCreateModel(CreateDifferenceKindDto dto);

    // Update istegini domain update modeline cevirir.
    public partial LookupUpdateModel MapToUpdateModel(UpdateDifferenceKindDto dto);

    // Dogrulanmis update modelini mevcut entity uzerine yazar; Id mapping disinda tutulur (kimlik asla mapleme ile degismez).
    [MapperIgnoreTarget(nameof(LookupEntity.Id))]
    public partial void MapToEntity(LookupUpdateModel model, [MappingTarget] DifferenceKind entity);
}
