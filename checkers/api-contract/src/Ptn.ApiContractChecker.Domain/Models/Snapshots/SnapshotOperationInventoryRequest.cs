namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Operasyon envanteri okumasinin kapali kume filtrelerini ve istenen sayfa penceresini tasir.
// sistemdeki gorevi: Manager'a serbest metin aramasi degil, sekli dogrulanmis sinirli secim girdisi ulastirir.
public sealed class SnapshotOperationInventoryRequest
{
    // Verildiginde envanteri tek bir kapali kume HTTP metoduna daraltir.
    public string? MethodCode { get; }

    // Verildiginde envanteri path sablonunun bu onekle basladigi operasyonlara daraltir.
    public string? PathPrefix { get; }

    // Verildiginde istek govdesi tasiyan veya tasimayan operasyonlari ayirir.
    public bool? HasRequestBody { get; }

    // Cagiranin istedigi sayfa baslangici.
    public int SkipCount { get; }

    // Cagiranin istedigi sayfa boyutu; etkili tavani manager uygular.
    public int MaxResultCount { get; }

    public SnapshotOperationInventoryRequest(
        string? methodCode,
        string? pathPrefix,
        bool? hasRequestBody,
        int skipCount,
        int maxResultCount)
    {
        MethodCode = methodCode;
        PathPrefix = pathPrefix;
        HasRequestBody = hasRequestBody;
        SkipCount = skipCount;
        MaxResultCount = maxResultCount;
    }
}
