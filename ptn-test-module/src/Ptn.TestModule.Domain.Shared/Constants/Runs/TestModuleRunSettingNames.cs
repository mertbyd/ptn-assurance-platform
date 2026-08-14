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
}
