using Ptn.DatabaseChecker.Dtos.Projections;
using Ptn.DatabaseChecker.Models.Projections;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Projections;

// islevi: Projection request/result DTO ve domain modelleri arasindaki donusumleri Mapperly ile uretir.
// sistemdeki gorevi: AppService'te elle property atamasi olmadan public sozlesmeyi domain cekirdegine baglar.
[Mapper]
public partial class ProjectionMapper
{
    // islevi: Public projection girdisini domain request modeline tasir.
    public partial ProjectionRequest MapToRequest(ProjectionRequestDto input);

    // islevi: Redaksiyonlu domain projection sonucunu public cevap DTO'suna tasir.
    public partial ProjectionResultDto MapToResultDto(ProjectionResult result);

    // islevi: Dictionary-tabanli domain satirini wrapper eklemeden public dictionary satirina tasir.
    private partial ProjectionRowDto MapRow(ProjectionRow row);
}
