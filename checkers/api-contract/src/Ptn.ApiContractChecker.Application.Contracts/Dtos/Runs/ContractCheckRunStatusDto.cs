using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Contract check run durumunu zaman ve denormalize sayaclarla hafif cevap olarak tasir.
// sistemdeki gorevi: Polling istemcilerinin findings JSON govdesini cekmeden ilerlemeyi izlemesini saglar.
public class ContractCheckRunStatusDto : EntityDto<Guid>
{
    public Guid CheckRunStatusId { get; set; }
    public string StatusCode { get; set; } = default!;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int BreakingCount { get; set; }
    public int NonBreakingCount { get; set; }
    public int DocsOnlyCount { get; set; }
}
