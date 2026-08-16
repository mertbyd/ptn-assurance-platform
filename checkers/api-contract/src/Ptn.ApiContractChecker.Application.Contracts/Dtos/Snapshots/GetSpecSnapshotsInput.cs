using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Snapshot gecmisinin ABP sayfalama girdisini ve rotadan gelen kaynak/dokuman kapsamini tasir.
// sistemdeki gorevi: Gecmis sorgusunun her zaman tek bir dokumana bagli kalmasini sozlesme seviyesinde zorunlu kilar.
public class GetSpecSnapshotsInput : PagedResultRequestDto
{
    public Guid SpecSourceId { get; set; }
    public Guid SpecDocumentId { get; set; }
}
