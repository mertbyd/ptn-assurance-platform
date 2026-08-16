namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Contract check gecmisindeki hafif run basligini owned bulgu govdesi olmadan tasir.
// sistemdeki gorevi: Liste ve durum sorgularinin JSON findings kolonunu materyalize etmeden sonuc dondurmesini saglar.
public class ContractCheckRunHeaderModel
{
    public Guid Id { get; set; }
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
