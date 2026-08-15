namespace Ptn.TestModule.Constants.Catalog;

// islevi: Katalog tarafindaki periyodik is ve zamanlama ayarlarinin kararli adlarini tanimlar.
// sistemdeki gorevi: Karantina supurucusu ile vade tarayicisinin ayni ABP Setting sozlesmesini kullanmasini saglar.
/// <summary>
/// Senaryo katalogu arka plan islerinin ABP Setting adlarini tasir.
/// </summary>
public static class TestModuleCatalogSettingNames
{
    /// <summary>Suresi dolmus karantina taramasinin iki tik arasindaki sure ayar adidir.</summary>
    public const string QuarantineSweepPeriodSeconds = "TestModule.Catalog.QuarantineSweepPeriodSeconds";

    /// <summary>Tek karantina tikinde temizlenecek azami senaryo sayisinin ayar adidir.</summary>
    public const string QuarantineSweepBatchSize = "TestModule.Catalog.QuarantineSweepBatchSize";

    /// <summary>Vadesi gelmis zamanlanmis senaryo taramasinin iki tik arasindaki sure ayar adidir.</summary>
    public const string ScheduleSweepPeriodSeconds = "TestModule.Catalog.ScheduleSweepPeriodSeconds";

    /// <summary>Tek zamanlama tikinde kuyruklanacak azami senaryo sayisinin ayar adidir.</summary>
    public const string MaxScenariosPerTick = "TestModule.Catalog.MaxScenariosPerTick";

    /// <summary>Varsayilan karantina tarama periyodudur.</summary>
    public const string DefaultQuarantineSweepPeriodSeconds = "300";

    /// <summary>Varsayilan karantina tik tavanidir.</summary>
    public const string DefaultQuarantineSweepBatchSize = "200";

    /// <summary>Varsayilan zamanlama tarama periyodudur.</summary>
    public const string DefaultScheduleSweepPeriodSeconds = "60";

    /// <summary>Varsayilan zamanlama tik tavanidir.</summary>
    public const string DefaultMaxScenariosPerTick = "50";

    /// <summary>Ayar okunamadiginda kullanilan zamanlama tarama periyodudur.</summary>
    public const int FallbackScheduleSweepPeriodSeconds = 60;

    /// <summary>Tek sozlesme degisikligi olayinda tetiklenebilecek azami senaryo sayisidir.</summary>
    public const int MaxContractChangeScenariosPerEvent = 500;

    /// <summary>Timer.Period milisaniye int'ine cevrildigi icin kabul edilen azami periyottur.</summary>
    public const int MaxWorkerPeriodSeconds = 3_600;

    /// <summary>Ayar okunamadiginda kullanilan karantina tarama periyodudur.</summary>
    public const int FallbackQuarantineSweepPeriodSeconds = 300;
}
