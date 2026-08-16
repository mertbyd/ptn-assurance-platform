namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Alinan bir spec anlik goruntusunun istemciye acilan kimlik ve surum bilgisini tasir.
// sistemdeki gorevi: Contract check tetiklemesine verilecek snapshot kimligini disari acan tek response sozlesmesidir.
public class SpecSnapshotDto
{
    // Contract check tetiklemesinde base/target olarak kullanilacak snapshot kimligi.
    public Guid Id { get; set; }

    // Snapshot'in alindigi kaynak dokumaninin kimligi.
    public Guid SpecDocumentId { get; set; }

    // Snapshot'in isaret ettigi degismez icerigin kimligi.
    public Guid SpecContentId { get; set; }

    // Okuyucu secimini belirleyen SpecFormat lookup kimligi.
    public Guid SpecFormatId { get; set; }

    // Spec info.version degeri; dokumanda bulunmayabilir.
    public string? ApiVersion { get; set; }

    // Ayni icerigin en son goruldugu zaman; degismeyen cekimde bu alan ilerler.
    public DateTime LastSeenAt { get; set; }

    // Snapshot satirinin acildigi zaman; LastSeenAt ile esitse icerik bu cekimde degismistir.
    public DateTime CreationTime { get; set; }
}
