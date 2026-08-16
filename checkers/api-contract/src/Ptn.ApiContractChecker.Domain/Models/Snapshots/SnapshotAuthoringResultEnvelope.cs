namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: ResultRef ile geri alinabilen tam operasyon veya sema ozetini tur koduyla tasir.
// sistemdeki gorevi: Ilk istegi yeniden calistirmadan kirpilmis yazarlik sonucunu geri verir.
public class SnapshotAuthoringResultEnvelope
{
    public string KindCode { get; set; } = string.Empty;
    public OperationSummaryResult? Operation { get; set; }
    public SchemaDescriptionResult? Schema { get; set; }
}
