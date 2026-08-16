namespace Ptn.ApiContractChecker.Constants.Snapshots;

// islevi: Snapshot operasyon/sema ozetleri ile resultRef omrunun kararli sinirlarini tanimlar.
// sistemdeki gorevi: Yazim ani cevaplarinin tam OpenAPI govdesine donusmesini engeller.
public static class SnapshotAuthoringConstants
{
    public const int MaxSummaryBytes = 2048;
    public const int MinimalFieldCount = 8;
    public const int NormalFieldCount = 24;
    public const int DefaultResultReferenceMinutes = 10;
    public const int MaxSchemaReferenceLength = 512;
    public const string OperationResultKind = "operation";
    public const string SchemaResultKind = "schema";
    public const string ResultCachePrefix = "api-contract:authoring-result:";
}
