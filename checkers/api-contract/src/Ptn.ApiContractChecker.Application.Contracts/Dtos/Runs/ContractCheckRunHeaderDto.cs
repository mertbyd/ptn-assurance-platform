using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Contract check gecmisindeki hafif run basligini API cevabi olarak tasir.
// sistemdeki gorevi: Liste ve status yuzeylerini owned findings govdesinden ayri tutar.
public class ContractCheckRunHeaderDto : EntityDto<Guid>
{
    public Guid BaseSnapshotId { get; set; }
    public Guid TargetSnapshotId { get; set; }
    public Guid CheckRunStatusId { get; set; }
    public string StatusCode { get; set; } = default!;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int BreakingCount { get; set; }
    public int NonBreakingCount { get; set; }
    public int DocsOnlyCount { get; set; }
    public DateTime CreationTime { get; set; }
}
