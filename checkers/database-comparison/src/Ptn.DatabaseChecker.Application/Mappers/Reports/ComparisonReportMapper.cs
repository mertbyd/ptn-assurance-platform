using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Findings;
using Ptn.DatabaseChecker.Dtos.Reports;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Reports;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Reports;

// islevi: Karsilastirma run entity'sini rapor cevap DTO'suna Mapperly ile tasir (baslik + detay findings).
// sistemdeki gorevi: Rapor header/detay eslemesinin tek noktasi; baglanti/motor/mod/durum adlari navigation'lardan otomatik duzlestirilir, Findings ic ice otomatik eslenir - elle property kopyalanmaz. Ozet + gruplar aggregation oldugu icin builder tarafindan disaridan set edilir (MapperIgnoreTarget).
[Mapper]
public partial class ComparisonReportMapper
{
    // islevi: Run entity'sini rapor DTO'suna tasir; navigation adlari duzlestirilir, aggregation alanlari disaridan doldurulur.
    // Entity anahtari Id, DTO alani RunId oldugu icin Mapperly ad eslemesi tutmaz; acikca eslenmezse RunId sessizce Guid.Empty kalir (rapor "hangi run" bilgisini kaybeder).
    [MapProperty(nameof(ComparisonRun.Id), nameof(ComparisonReportDto.RunId))]
    [MapProperty("Definition.Name", nameof(ComparisonReportDto.ComparisonDefinitionName))]
    [MapProperty("SourceConnection.Engine.Code", nameof(ComparisonReportDto.SourceEngineCode))]
    [MapProperty("SourceConnection.Engine.Name", nameof(ComparisonReportDto.SourceEngineName))]
    [MapProperty("TargetConnection.Engine.Code", nameof(ComparisonReportDto.TargetEngineCode))]
    [MapProperty("TargetConnection.Engine.Name", nameof(ComparisonReportDto.TargetEngineName))]
    [MapperIgnoreTarget(nameof(ComparisonReportDto.Summary))]
    [MapperIgnoreTarget(nameof(ComparisonReportDto.ObjectTypeGroups))]
    [MapperIgnoreTarget(nameof(ComparisonReportDto.TableGroups))]
    public partial ComparisonReportDto MapToDto(ComparisonRun run);

    // islevi: Migration finding modelindeki kaynak/hedef sema ve surum bilgisini rapor API DTO'suna tasir.
    public partial MigrationDifferenceDto MapToDto(MigrationDifferenceModel difference);

    // islevi: Builder'in urettigi ozet aggregation modelini rapor ozet DTO'suna tasir (yon/nesne-turu sayaclari ic ice eslenir).
    public partial ComparisonReportSummaryDto MapToDto(ComparisonReportSummaryModel summary);

    // islevi: Builder'in urettigi grup modeli listesini rapor grup DTO listesine tasir (tur ve tablo gruplari ayni tipten beslenir).
    public partial List<ComparisonReportGroupDto> MapToDto(List<ComparisonReportGroupModel> groups);
}
