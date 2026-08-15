namespace Ptn.TestModule.Constants.Catalog;

// islevi: Senaryo katalogunun metin, hash ve veritabani indeks sinirlarini tanimlar.
// sistemdeki gorevi: DTO dogrulamasi, Manager normalizasyonu ve EF konfigurasyonunun DBML ile ayni sozlesmeyi kullanmasini saglar.
public static class TestScenarioConsts
{
    public const int MaxScenarioKeyLength = 128;
    public const int MaxTitleLength = 256;
    public const int MaxDescriptionLength = 1024;
    public const int HashLength = 64;
    public const int MaxDerivabilityCodeLength = 64;
    public const int MaxAgentModelRefLength = 64;
    public const int MaxNotesLength = 1024;

    public const string ScenarioKeyPattern = "^[a-z0-9][a-z0-9._-]{0,127}$";
    public const string HashPattern = "^[a-fA-F0-9]{64}$";

    public const string VersionIndexName = "ux_scenarios_version";
    public const string ContentIndexName = "ux_scenarios_content";
    public const string StateIndexName = "ix_scenarios_state";

    /// <summary>Aktif karantina sorgularinin indeks adidir.</summary>
    public const string QuarantineIndexName = "ix_scenarios_quarantine";

    /// <summary>Karantina gerekcesinin azami karakter sayisidir.</summary>
    public const int MaxQuarantineReasonLength = 256;

    /// <summary>Bir senaryonun kesintisiz kalabilecegi azami karantina gunudur.</summary>
    public const int MaxQuarantineDays = 30;
}
