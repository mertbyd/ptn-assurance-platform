using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Definitions;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Models.Definitions;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Definitions;

// islevi: Karsilastirma tanimi DTO/model/entity donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Tanimin create/update ve read alan tasimasini Application katmaninda tek merkezde tutar.
[Mapper]
public partial class ComparisonDefinitionMapper
{
    // islevi: Karsilastirma tanimi entity bilgisini API cevap DTO'suna tasir.
    public partial ComparisonDefinitionDto MapToDto(ComparisonDefinition entity);

    // islevi: Karsilastirma tanimi entity listesini API cevap DTO listesine tasir.
    public partial List<ComparisonDefinitionDto> MapToDto(List<ComparisonDefinition> entities);

    // islevi: Karsilastirma tanimi olusturma istegini domain manager modeline tasir.
    public partial CreateComparisonDefinitionModel MapToCreateModel(CreateComparisonDefinitionDto dto);

    // islevi: Karsilastirma tanimi guncelleme istegini domain manager modeline tasir.
    public partial UpdateComparisonDefinitionModel MapToUpdateModel(UpdateComparisonDefinitionDto dto);

    // islevi: Manager tarafindan dogrulanmis tanim olusturma modelini yeni entity uzerine uygular.
    [MapperIgnoreTarget(nameof(ComparisonDefinition.ComparisonType))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.CreationTime))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.CreatorId))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.LastModificationTime))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.LastModifierId))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.SourceConnection))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.TargetConnection))]
    public partial void MapToEntity(CreateComparisonDefinitionModel model, [MappingTarget] ComparisonDefinition entity);

    // islevi: Manager tarafindan dogrulanmis tanim guncelleme modelini mevcut entity uzerine uygular.
    [MapperIgnoreTarget(nameof(ComparisonDefinition.ComparisonType))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.CreationTime))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.CreatorId))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.LastModificationTime))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.LastModifierId))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.SourceConnection))]
    [MapperIgnoreTarget(nameof(ComparisonDefinition.TargetConnection))]
    public partial void MapToEntity(UpdateComparisonDefinitionModel model, [MappingTarget] ComparisonDefinition entity);
}
