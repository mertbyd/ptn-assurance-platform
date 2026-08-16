using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Lookups;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Models.Lookups;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Lookups;

// islevi: Fark guveni (Exact/Canonical/Approximate/Incomparable) lookup'inin DTO/model/entity donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Elle property kopyalamayi engeller; tum lookup CRUD akisinin mapping'i bu sinifta toplanir.
[Mapper]
public partial class ComparisonConfidenceMapper
{
    // Entity'yi API cikti DTO'suna cevirir.
    public partial ComparisonConfidenceDto MapToDto(ComparisonConfidence entity);

    // Listeleme icin entity koleksiyonunu DTO listesine cevirir.
    public partial List<ComparisonConfidenceDto> MapToDto(List<ComparisonConfidence> entities);

    // Create istegini domain create modeline cevirir.
    public partial LookupCreateModel MapToCreateModel(CreateComparisonConfidenceDto dto);

    // Update istegini domain update modeline cevirir.
    public partial LookupUpdateModel MapToUpdateModel(UpdateComparisonConfidenceDto dto);

    // Dogrulanmis update modelini mevcut entity uzerine yazar; Id mapping disinda tutulur (kimlik asla mapleme ile degismez).
    [MapperIgnoreTarget(nameof(LookupEntity.Id))]
    public partial void MapToEntity(LookupUpdateModel model, [MappingTarget] ComparisonConfidence entity);
}
