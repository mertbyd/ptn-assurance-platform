using Ptn.ApiContractChecker.Dtos.Lookups;
using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Tek anlik goruntuyu bagli icerik ve format satirlariyla birlikte API cevabi olarak tasir.
// sistemdeki gorevi: Entity grafigini birebir aynalar; yeni alan acildiginda eslemenin degismesi gerekmez.
public class SpecSnapshotDetailDto : EntityDto<Guid>
{
    // Snapshot'in alindigi kaynak dokumaninin kimligi.
    public Guid SpecDocumentId { get; set; }

    // Snapshot'in isaret ettigi degismez icerigin kimligi.
    public Guid SpecContentId { get; set; }

    // Okuyucu secimini belirleyen SpecFormat lookup kimligi.
    public Guid SpecFormatId { get; set; }

    // Spec info.version degeri; dokumanda bulunmayabilir.
    public string? ApiVersion { get; set; }

    // Ayni icerigin en son goruldugu zaman.
    public DateTime LastSeenAt { get; set; }

    // Snapshot satirinin acildigi zaman.
    public DateTime CreationTime { get; set; }

    // Ham spec metni ve olculeri; detay yolunun ayri alani olarak doner.
    public SpecContentDto SpecContent { get; set; } = default!;

    // Snapshot'in bagli oldugu format lookup satiri.
    public SpecFormatDto SpecFormat { get; set; } = default!;
}
