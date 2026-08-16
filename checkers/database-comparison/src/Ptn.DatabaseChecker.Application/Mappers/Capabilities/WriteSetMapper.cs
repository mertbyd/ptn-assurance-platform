using Ptn.DatabaseChecker.Dtos.Capabilities;
using Ptn.DatabaseChecker.Models.Capabilities;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.Capabilities;

// islevi: Write-set probe/capture DTO ve domain modelleri arasindaki donusumleri Mapperly ile uretir.
// sistemdeki gorevi: AppService'te elle property atamasi veya provider modeli sizintisi olmasini engeller.
[Mapper]
public partial class WriteSetMapper
{
    // islevi: Public probe girdisini domain request modeline tasir.
    public partial CapabilityProbeRequest MapToRequest(CapabilityProbeRequestDto input);

    // islevi: Public capture girdisini domain request modeline tasir.
    public partial WriteSetCaptureRequest MapToRequest(WriteSetCaptureRequestDto input);

    // islevi: Domain capability seviyesini public response DTO'suna tasir.
    public partial CapabilityLevelDto MapToLevelDto(CapabilityLevel input);

    // islevi: Domain advisory footprint sonucunu public response DTO'suna tasir.
    public partial WriteSetResultDto MapToResultDto(WriteSetResult input);

    // islevi: Domain tablo deltasini public response item'ina tasir.
    private partial WriteSetTableDeltaDto MapDelta(WriteSetTableDelta input);
}
