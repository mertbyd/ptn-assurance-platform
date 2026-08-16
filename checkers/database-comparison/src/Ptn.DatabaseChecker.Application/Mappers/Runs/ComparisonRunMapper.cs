using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.Dtos.Findings;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Models.Runs;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Runs;

// islevi: Karsilastirma calisma kaydi projeksiyon/entity/DTO/model donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Hafif header projeksiyonunu liste DTO'suna, tam entity'yi agir detay DTO'suna tasir; elle property kopyalamayi engeller.
[Mapper]
public partial class ComparisonRunMapper
{
    // islevi: Hafif header projeksiyon modelini liste/get DTO'suna tasir (owned veri icermez).
    public partial ComparisonRunDto MapToDto(ComparisonRunHeaderModel header);

    // islevi: Header projeksiyon listesini liste DTO listesine tasir.
    public partial List<ComparisonRunDto> MapToDto(List<ComparisonRunHeaderModel> headers);

    /// <summary>
    /// Bulgu sorgu DTO'sunu repository/domain filtre modeline tasir.
    /// </summary>
    public partial FindingQueryModel MapToQueryModel(FindingQueryInput input);

    /// <summary>
    /// Ortak bulgu projection listesini MCP cevap DTO listesine tasir.
    /// </summary>
    public partial List<FindingDto> MapToFindingDtos(List<FindingReadModel> findings);

    // islevi: Tam entity'yi (navigation + owned bulgular/rapor) agir detay DTO'suna tasir.
    // Definition navigation adi DTO'daki ComparisonDefinitionName ile uyusmadigi icin (nullable) acikca eslenir; ad-hoc run'da null kalir.
    [MapProperty("Definition.Name", nameof(ComparisonRunDetailDto.ComparisonDefinitionName))]
    public partial ComparisonRunDetailDto MapToDetailDto(ComparisonRun entity);

    // islevi: Karsilastirma calisma olusturma istegini domain manager modeline tasir.
    public partial CreateComparisonRunModel MapToCreateModel(CreateComparisonRunDto dto);

    // islevi: Manager tarafindan dogrulanmis calisma olusturma modelini yeni entity uzerine uygular.
    [MapperIgnoreTarget(nameof(ComparisonRun.ComparisonType))]
    [MapperIgnoreTarget(nameof(ComparisonRun.CompletedAt))]
    [MapperIgnoreTarget(nameof(ComparisonRun.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(ComparisonRun.CreationTime))]
    [MapperIgnoreTarget(nameof(ComparisonRun.CreatorId))]
    [MapperIgnoreTarget(nameof(ComparisonRun.DataDifferenceCount))]
    [MapperIgnoreTarget(nameof(ComparisonRun.Definition))]
    [MapperIgnoreTarget(nameof(ComparisonRun.ErrorMessage))]
    [MapperIgnoreTarget(nameof(ComparisonRun.Findings))]
    [MapperIgnoreTarget(nameof(ComparisonRun.LastModificationTime))]
    [MapperIgnoreTarget(nameof(ComparisonRun.LastModifierId))]
    [MapperIgnoreTarget(nameof(ComparisonRun.MigrationDifferenceCount))]
    [MapperIgnoreTarget(nameof(ComparisonRun.Reports))]
    [MapperIgnoreTarget(nameof(ComparisonRun.SchemaDifferenceCount))]
    [MapperIgnoreTarget(nameof(ComparisonRun.SourceConnection))]
    [MapperIgnoreTarget(nameof(ComparisonRun.StartedAt))]
    [MapperIgnoreTarget(nameof(ComparisonRun.Status))]
    [MapperIgnoreTarget(nameof(ComparisonRun.TargetConnection))]
    public partial void MapToEntity(CreateComparisonRunModel model, [MappingTarget] ComparisonRun entity);

    // islevi: Motorun urettigi calistirma sonuc modelini (snapshot FK + zaman + sayaclar + bulgular) yeni entity uzerine uygular.
    // Owned Findings ayni tipte oldugu icin Mapperly derin kopyalar; navigation/audit/Reports/ErrorMessage hedefleri motor girdisi olmadigi icin bilerek eslenmez (EF/ABP yonetir).
    [MapperIgnoreTarget(nameof(ComparisonRun.Definition))]
    [MapperIgnoreTarget(nameof(ComparisonRun.SourceConnection))]
    [MapperIgnoreTarget(nameof(ComparisonRun.TargetConnection))]
    [MapperIgnoreTarget(nameof(ComparisonRun.ComparisonType))]
    [MapperIgnoreTarget(nameof(ComparisonRun.Status))]
    [MapperIgnoreTarget(nameof(ComparisonRun.ErrorMessage))]
    [MapperIgnoreTarget(nameof(ComparisonRun.Reports))]
    [MapperIgnoreTarget(nameof(ComparisonRun.TenantId))]
    [MapperIgnoreTarget(nameof(ComparisonRun.LastModificationTime))]
    [MapperIgnoreTarget(nameof(ComparisonRun.LastModifierId))]
    [MapperIgnoreTarget(nameof(ComparisonRun.CreationTime))]
    [MapperIgnoreTarget(nameof(ComparisonRun.CreatorId))]
    [MapperIgnoreTarget(nameof(ComparisonRun.ExtraProperties))]
    [MapperIgnoreTarget(nameof(ComparisonRun.ConcurrencyStamp))]
    public partial void MapExecutionResultToEntity(ComparisonRunExecutionResultModel model, [MappingTarget] ComparisonRun entity);
}
