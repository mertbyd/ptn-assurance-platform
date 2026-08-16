namespace Ptn.ApiContractChecker.Models.Sources;

// islevi: Bir dokumanin zamanlanmis izleme tercihini ve kontrol araligini mutasyon girdisi olarak tasir.
// sistemdeki gorevi: Izleme kararini DTO'yu Domain'e sokmadan aggregate davranisina iletir; ad+path guncelleme yolundan ayri kalir.
public class SpecDocumentMonitoringModel
{
    // Dokumanin zamanlanmis taramaya girip girmeyecegi.
    public bool IsMonitored { get; set; }

    // Izleme aciksa iki kontrol arasindaki bekleme; kapaliyken deger tasinmaz.
    public int? CheckIntervalMinutes { get; set; }
}
