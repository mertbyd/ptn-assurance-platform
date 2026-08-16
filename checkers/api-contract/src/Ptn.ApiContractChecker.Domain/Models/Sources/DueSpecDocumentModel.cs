namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Vadesi gelmis tek bir izlenen dokumani kimlik ve tenant sahipligiyle tasir.
// sistemdeki gorevi: Worker'in capraz tenant tarama sonucunu aggregate govdesi yuklemeden kuyruga cevirmesini saglar.
public class DueSpecDocumentModel
{
    // Dokumanin sahibi kaynak aggregate'inin kimligi.
    public Guid SpecSourceId { get; set; }

    // Vadesi gelmis dokumanin kimligi.
    public Guid SpecDocumentId { get; set; }

    // Job kuyruklanirken geri acilacak tenant baglami.
    public Guid? TenantId { get; set; }
}
