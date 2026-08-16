namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Karsilastirmaya giren tek snapshot'in insan-okur kaynak, dokuman, surum ve gorulme kimligini tasir.
// sistemdeki gorevi: Bildirim metninin hangi iki fotografin karsilastirildigini Guid yerine adla anlatmasini saglar.
public class ContractCheckRunSnapshotSummaryModel
{
    // Snapshot'in dokumanini yayimlayan kaynagin adi.
    public string SourceName { get; set; } = default!;

    // Kaynak icindeki dokumanin gorunen adi.
    public string DocumentName { get; set; } = default!;

    // Spec info.version degeri; dokumanda bulunmayabilir.
    public string? ApiVersion { get; set; }

    // Ayni ham icerigin en son goruldugu zaman; surum yoksa iki snapshot'i ayiran tek isaret budur.
    public DateTime TakenAt { get; set; }
}
