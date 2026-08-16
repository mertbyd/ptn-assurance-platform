using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Snapshot gecmisi listesindeki tek satiri API cevabi olarak tasir.
// sistemdeki gorevi: Karsilastirma ekraninin snapshot secimini ham spec govdesi tasimadan besler.
public class SpecSnapshotHeaderDto : EntityDto<Guid>
{
    // Snapshot satirinin acildigi, yani icerigin ilk kez bu haliyle goruldugu zaman.
    public DateTime CreationTime { get; set; }

    // Ayni icerigin en son goruldugu zaman; degismeyen cekimde bu alan ilerler.
    public DateTime LastSeenAt { get; set; }

    // Okuyucu secimini belirleyen kararli SpecFormat kodu.
    public string FormatCode { get; set; } = default!;

    // Spec info.version degeri; dokumanda bulunmayabilir.
    public string? ApiVersion { get; set; }

    // Ham icerigin UTF-8 bayt boyutu.
    public int ByteSize { get; set; }

    // Iki satiri gozle ayirt etmeye yeten kanonik hash on eki.
    public string ShortCanonicalHash { get; set; } = default!;
}
