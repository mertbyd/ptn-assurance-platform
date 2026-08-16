namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Vadesi ilerletilmis bir dokumanin cekim istegini ve degisim karsilastirmasinin taban snapshot'ini tasir.
// sistemdeki gorevi: Kisa UOW'de okunan her seyi tasiyip UOW'suz cekim adiminin repository'ye donmesini gereksiz kilar.
public class ScheduledDocumentCheckContextModel
{
    // Cekim sonucunun yazilacagi aggregate kokunun kimligi.
    public Guid SpecSourceId { get; set; }

    // Cekim sonrasi ingest edilecek dokumanin kimligi.
    public Guid SpecDocumentId { get; set; }

    // Canli govdenin alinacagi adres ve opsiyonel Vault yolu.
    public SpecFetchRequestModel FetchRequest { get; set; } = default!;

    // Cekim oncesindeki son snapshot; yeni satir acilirsa karsilastirmanin taban tarafi budur.
    public Guid? PreviousSnapshotId { get; set; }
}
