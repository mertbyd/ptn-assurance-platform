using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: ABP bulgu sayfasini byte butcesi ve acik kirpilma metadatasiyla tasir.
// sistemdeki gorevi: Sessiz sayfa kirpmayi engellerken standart TotalCount ve Items sozlesmesini korur.
public class FindingPagedResultDto : PagedResultDto<FindingDto>
{
    public int RequestedMaxResultCount { get; set; }
    public int EffectiveMaxResultCount { get; set; }
    public bool IsTruncated { get; set; }
    public int ResponseBytes { get; set; }
}
