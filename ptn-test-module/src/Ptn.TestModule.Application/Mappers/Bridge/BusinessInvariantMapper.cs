using Ptn.TestModule.Dtos.Bridge.Invariants;
using Ptn.TestModule.Models.Bridge.Invariants;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Is degismezi request/response DTO'lariyla domain modellerini compile-time esler.
// sistemdeki gorevi: Degismez use-case'indeki tum alan kopyalamanin tek bildirime dayali sahibidir.
[Mapper]
public partial class BusinessInvariantMapper
{
    public partial BusinessInvariantRequest Map(BusinessInvariantRequestDto input);
    public partial BusinessInvariantResultDto Map(BusinessInvariantResult input);
}
