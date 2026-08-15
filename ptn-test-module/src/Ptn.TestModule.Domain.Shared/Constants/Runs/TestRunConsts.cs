namespace Ptn.TestModule.Constants.Runs;

// islevi: Kosum kaydinin metin, hash, trace ve indeks sozlesmesini tanimlar.
// sistemdeki gorevi: Manager dogrulamasi ile EF Core eslemesini DBML sinirlarinda birlestirir.
/// <summary>
/// Test kosum kayitlarinin kararli uzunluk, desen, malzeme ve indeks sabitlerini tasir.
/// </summary>
public static class TestRunConsts
{
    /// <summary>Test anahtarinin azami karakter sayisidir.</summary>
    public const int MaxTestKeyLength = 128;

    /// <summary>SHA-256 degerlerinin sabit karakter sayisidir.</summary>
    public const int HashLength = 64;

    /// <summary>W3C trace kimliginin sabit karakter sayisidir.</summary>
    public const int TraceIdLength = 32;

    /// <summary>Mantiksal ortam anahtarinin azami karakter sayisidir.</summary>
    public const int MaxEnvironmentKeyLength = 128;

    /// <summary>Tetikleyici referansinin azami karakter sayisidir.</summary>
    public const int MaxTriggerRefLength = 256;

    /// <summary>Runner surum referansinin azami karakter sayisidir.</summary>
    public const int MaxRunnerRefLength = 64;

    /// <summary>HAR blob adinin azami karakter sayisidir.</summary>
    public const int MaxHarBlobNameLength = 256;

    /// <summary>Kucuk harfli W3C trace-id bicimini dogrulayan desendir.</summary>
    public const string TraceIdPattern = "^[a-f0-9]{32}$";

    /// <summary>Kucuk harfli SHA-256 fingerprint bicimini dogrulayan desendir.</summary>
    public const string FingerprintPattern = "^[a-f0-9]{64}$";

    /// <summary>Test anahtari sorgularinin indeks adidir.</summary>
    public const string TestIndexName = "ix_runs_test";

    /// <summary>Trend kovasi sorgularinin indeks adidir.</summary>
    public const string TrendIndexName = "ix_runs_trend";

    /// <summary>Asili Running kosularini bulan indeks adidir.</summary>
    public const string StaleIndexName = "ix_runs_stale";

    /// <summary>Trace kimligi sorgularinin indeks adidir.</summary>
    public const string TraceIndexName = "ix_runs_trace";

    /// <summary>Kurallar dokumani malzemesinin rapor kodudur.</summary>
    public const string RulesMaterialCode = "Rules";

    /// <summary>API snapshot malzemesinin rapor kodudur.</summary>
    public const string ApiSpecificationMaterialCode = "ApiSpecification";

    /// <summary>Veritabani semasi malzemesinin rapor kodudur.</summary>
    public const string DatabaseSchemaMaterialCode = "DatabaseSchema";

    /// <summary>Profil paketi malzemesinin rapor kodudur.</summary>
    public const string ProfileMaterialCode = "Profile";
}
