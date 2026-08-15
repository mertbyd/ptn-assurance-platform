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

    /// <summary>Varsayilan karantina tarama periyodudur.</summary>
    public const string DefaultQuarantineSweepPeriodSeconds = "300";

    /// <summary>Varsayilan karantina tik tavanidir.</summary>
    public const string DefaultQuarantineSweepBatchSize = "200";

    /// <summary>Timer.Period milisaniye int'ine cevrildigi icin kabul edilen azami periyottur.</summary>
    public const int MaxWorkerPeriodSeconds = 3_600;

    /// <summary>Ayar okunamadiginda kullanilan karantina tarama periyodudur.</summary>
    public const int FallbackQuarantineSweepPeriodSeconds = 300;
}
