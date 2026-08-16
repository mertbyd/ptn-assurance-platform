namespace Ptn.ApiContractChecker.Configuration;

// islevi: Zamanlanmis dokuman izlemesinin tarama sikligini ve bir tikte kuyruga alinacak azami dokuman sayisini typed options olarak tasir.
// sistemdeki gorevi: Worker'in yuku appsettings ile ayarlanabilir kalir; deger anlamliligi SpecMonitoringOptionsValidator'in isidir.
public class SpecMonitoringOptions
{
    public const string SectionName = "SpecMonitoring";
    public const int DefaultWorkerPeriodSeconds = 60;
    public const int DefaultMaxDocumentsPerTick = 50;

    // Timer.Period milisaniye int'ine cevrilecegi icin ust sinir ayrica dogrulanir.
    public const int MaxWorkerPeriodSeconds = 3_600;

    // Vadesi gelmis dokumanlarin tarandigi iki tik arasindaki sure.
    public int WorkerPeriodSeconds { get; set; } = DefaultWorkerPeriodSeconds;

    // Tek tikte kuyruga alinacak azami dokuman; geri kalanlar sonraki tiklerde sirasini alir.
    public int MaxDocumentsPerTick { get; set; } = DefaultMaxDocumentsPerTick;
}
