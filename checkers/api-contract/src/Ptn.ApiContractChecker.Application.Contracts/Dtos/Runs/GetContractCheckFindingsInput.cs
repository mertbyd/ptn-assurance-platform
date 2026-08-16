using Ptn.ApiContractChecker.Constants.Runs;
using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Tek run bulgularinin ABP sayfalama ve bakim ani filtrelerini tasir.
// sistemdeki gorevi: Bulgu govdesinin tek parca alinmasini engelleyen public query sozlesmesidir.
public class GetContractCheckFindingsInput : PagedResultRequestDto
{
    public string? SeverityCode { get; set; }
    public string? KindCode { get; set; }
    public string? ChangeStateCode { get; set; }
    public string? Path { get; set; }
    public string? SchemaName { get; set; }
    public Guid? SinceRunId { get; set; }
    public List<string> Fingerprints { get; set; } = [];

    public GetContractCheckFindingsInput()
    {
        MaxResultCount = ContractCheckRunConsts.DefaultFindingPageSize;
    }
}
