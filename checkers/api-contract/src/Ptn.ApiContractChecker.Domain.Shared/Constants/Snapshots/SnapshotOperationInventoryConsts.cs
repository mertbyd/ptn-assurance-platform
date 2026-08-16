namespace Ptn.ApiContractChecker.Constants.Snapshots;

// islevi: Operasyon envanteri okumasinin sayfa adedi, UTF-8 cikti tavani ve path onek uzunlugu sinirlarini tanimlar.
// sistemdeki gorevi: Validator, manager ve cevap butcesinin ayni esikleri kullanmasini saglar; ajan yuzeyinde sinirsiz liste birakmaz.
public static class SnapshotOperationInventoryConsts
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MaxResponseBytes = 32 * 1024;
    public const int MaxPathPrefixLength = 256;
}
