namespace Ptn.ApiContractChecker.BackgroundJobs.Sources;

// islevi: Vadesi gelmis tek bir dokumanin zamanlanmis kontrolu icin yeniden teslim edilebilir payload'i tasir.
// sistemdeki gorevi: Worker'in tarama sonucunu, tenant baglamiyla birlikte ABP kuyruguna aktarir.
public class ScheduledDocumentCheckJobArgs : ITenantBackgroundJobArgs
{
    // Kontrol edilecek dokumanin sahibi kaynak aggregate'i.
    public Guid SpecSourceId { get; set; }

    // Vadesi gelmis dokumanin kimligi.
    public Guid SpecDocumentId { get; set; }

    // Job baslarken acilacak tenant baglami.
    public Guid? TenantId { get; set; }
}
