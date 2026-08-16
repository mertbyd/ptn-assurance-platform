namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: ResultRef ile geri alinan tam operasyon veya sema ozetini tur koduyla HTTP cevabinda tasir.
public class SnapshotAuthoringResultDto
{
    public string KindCode { get; set; } = string.Empty;
    public OperationSummaryDto? Operation { get; set; }
    public SchemaDescriptionDto? Schema { get; set; }
}
