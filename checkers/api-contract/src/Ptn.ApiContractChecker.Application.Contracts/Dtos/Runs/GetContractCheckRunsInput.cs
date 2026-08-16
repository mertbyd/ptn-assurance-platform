using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Contract check gecmisinin ABP sayfalama ve opsiyonel dokuman/kaynak filtrelerini tasir.
// sistemdeki gorevi: GET query parametrelerini siralama sozlesmesi acmadan AppService'e aktarir.
public class GetContractCheckRunsInput : PagedResultRequestDto
{
    public Guid? SpecDocumentId { get; set; }
    public Guid? SpecSourceId { get; set; }
}
