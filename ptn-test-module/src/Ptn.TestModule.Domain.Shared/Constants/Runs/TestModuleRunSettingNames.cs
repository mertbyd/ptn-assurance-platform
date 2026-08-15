namespace Ptn.TestModule.Constants.Runs;

// islevi: Tenant-scoped ortam baglama ayarinin ve JSON alanlarinin kararli adlarini tanimlar.
// sistemdeki gorevi: Setting provider ile RunEnvironmentBindingManager'in ayni guvenli ortam sozlesmesini kullanmasini saglar.
/// <summary>
/// Test kosum ortam baglamalarinin ABP Setting ve JSON alan adlarini tasir.
/// </summary>
public static class TestModuleRunSettingNames
{
    /// <summary>Tum mantiksal ortam baglamalarini tutan tenant-scoped setting adidir.</summary>
    public const string EnvironmentBindings = "TestModule.Runs.EnvironmentBindings";

    /// <summary>Varsayilan bos ortam baglama haritasidir.</summary>
    public const string DefaultEnvironmentBindings = "{}";

    /// <summary>API tarafinin JSON nesne adidir.</summary>
    public const string ApiSection = "api";

    /// <summary>Veritabani tarafinin JSON nesne adidir.</summary>
    public const string DatabaseSection = "database";

    /// <summary>Her iki tarafta acikca yazilan ortam anahtari alanidir.</summary>
    public const string EnvironmentKey = "environmentKey";

    /// <summary>Sistemin API taban adresi alanidir.</summary>
    public const string BaseUrl = "baseUrl";

    /// <summary>API checker snapshot kimligi alanidir.</summary>
    public const string SpecSnapshotId = "specSnapshotId";

    /// <summary>Database Checker baglanti kimligi alanidir.</summary>
    public const string DbConnectionId = "dbConnectionId";

    /// <summary>Secret degeri yerine saklanan mantiksal referans alanidir.</summary>
    public const string SecretRef = "secretRef";

    /// <summary>Kosumu icra eden pinli dis runner imajinin ayar adidir.</summary>
    public const string RunnerImage = "TestModule.Runs.RunnerImage";

    /// <summary>Tum senaryonun runner tarafindaki azami sure ayar adidir.</summary>
    public const string RunnerExecutionTimeoutSeconds = "TestModule.Runs.RunnerExecutionTimeoutSeconds";

    /// <summary>Tek HTTP cagrisinin runner tarafindaki azami sure ayar adidir.</summary>
    public const string RunnerMaxFetchTimeoutSeconds = "TestModule.Runs.RunnerMaxFetchTimeoutSeconds";

    /// <summary>HAR artefaktlarinin gun cinsinden saklama suresi ayar adidir.</summary>
    public const string HarRetentionDays = "TestModule.Runs.HarRetentionDays";

    /// <summary>Asili Running kosularinin kurtarilma esigi dakika ayar adidir.</summary>
    public const string StaleRunThresholdMinutes = "TestModule.Runs.StaleRunThresholdMinutes";

    /// <summary>Sandbox verisini kosumdan once temizleyen stratejinin ayar adidir.</summary>
    public const string SandboxResetStrategy = "TestModule.Runs.SandboxResetStrategy";

    /// <summary>Desteklenen iliskisel sandbox temizleme stratejisidir.</summary>
    public const string RespawnResetStrategy = "Respawn";

    /// <summary>Ortam bazli ayri sandbox connection-string adinin bicimidir.</summary>
    public const string SandboxConnectionStringNameFormat = "TestModuleSandbox.{0}";

    /// <summary>Connection-string adina guvenle tasinabilen ortam anahtari bicimidir.</summary>
    public const string SandboxEnvironmentKeyPattern = "^[A-Za-z0-9._-]+$";

    /// <summary>Varsayilan HAR saklama suresidir.</summary>
    public const string DefaultHarRetentionDays = "30";

    /// <summary>Varsayilan asili kosum kurtarma esigidir.</summary>
    public const string DefaultStaleRunThresholdMinutes = "120";

    /// <summary>Varsayilan sandbox temizleme stratejisidir.</summary>
    public const string DefaultSandboxResetStrategy = RespawnResetStrategy;
}
