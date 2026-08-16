namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Icerik degistiginde karsilastirilacak iki snapshot kimligini tasir.
// sistemdeki gorevi: Zamanlanmis kontrolun dedup sonucundan cikardigi cifti, mevcut check tetikleme yoluna hesaplamadan teslim eder.
public class ScheduledDocumentCheckPairModel
{
    // Cekim oncesindeki son snapshot.
    public Guid BaseSnapshotId { get; set; }

    // Cekimin actigi yeni snapshot.
    public Guid TargetSnapshotId { get; set; }
}
