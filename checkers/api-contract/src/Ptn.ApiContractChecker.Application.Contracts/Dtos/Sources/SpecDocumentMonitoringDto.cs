using Volo.Abp.Application.Dtos;

namespace Ptn.ApiContractChecker.Dtos.Sources;

// islevi: Bir dokumanin guncel izleme durumunu ve hesaplanan vadesini yanitta tasir.
// sistemdeki gorevi: Izleme alanlarini kaynak mutasyon girdisi olan SpecDocumentDto'ya karistirmadan cagirana geri bildirir.
public class SpecDocumentMonitoringDto : EntityDto<Guid>
{
    // Dokumanin zamanlanmis taramaya girip girmedigi.
    public bool IsMonitored { get; set; }

    // Izleme aciksa iki kontrol arasindaki bekleme.
    public int? CheckIntervalMinutes { get; set; }

    // Worker'in vadesi geldi karari verecegi zaman.
    public DateTime? NextCheckAt { get; set; }

    // Son zamanlanmis kontrol denemesinin zamani.
    public DateTime? LastCheckedAt { get; set; }
}
