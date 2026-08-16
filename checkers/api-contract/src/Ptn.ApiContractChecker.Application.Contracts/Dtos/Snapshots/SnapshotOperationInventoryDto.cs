using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: ABP operasyon sayfasini kapali sonuc kodu, byte butcesi ve acik kirpilma metadatasiyla tasir.
// sistemdeki gorevi: Kapsam raporunun paydasini verirken sessiz sayfa kirpmayi engeller.
public class SnapshotOperationInventoryDto : PagedResultDto<SnapshotOperationRowDto>
{
    public string OutcomeCode { get; set; } = string.Empty;
    public int RequestedMaxResultCount { get; set; }
    public int EffectiveMaxResultCount { get; set; }
    public bool IsTruncated { get; set; }
    public int ResponseBytes { get; set; }
}
