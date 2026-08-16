namespace Ptn.ApiContractChecker.Dtos.Sources;

// islevi: Tek bir dokumanin zamanlanmis izleme tercihini ve kontrol araligini istek govdesinde tasir.
// sistemdeki gorevi: Zamanlamayi kaynak guncelleme sozlesmesinden ayri tutar; ad+path guncellemesi izlemeyi hic gormez.
public class ConfigureSpecDocumentMonitoringDto
{
    // Dokumanin zamanlanmis taramaya girip girmeyecegi.
    public bool IsMonitored { get; set; }

    // Izleme aciksa zorunlu olan dakika cinsinden kontrol araligi.
    public int? CheckIntervalMinutes { get; set; }
}
