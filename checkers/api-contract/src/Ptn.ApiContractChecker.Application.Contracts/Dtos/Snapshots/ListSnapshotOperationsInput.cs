using Ptn.ApiContractChecker.Constants.Snapshots;
using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Snapshot operasyon envanterinin ABP sayfalama ve kapali kume filtrelerini tasir.
// sistemdeki gorevi: Serbest metin aramasi acmadan, checker'in urettigi seceneklerle daraltilan public query sozlesmesidir.
public class ListSnapshotOperationsInput : PagedResultRequestDto
{
    public string? MethodCode { get; set; }
    public string? PathPrefix { get; set; }
    public bool? HasRequestBody { get; set; }

    public ListSnapshotOperationsInput()
    {
        MaxResultCount = SnapshotOperationInventoryConsts.DefaultPageSize;
    }
}
