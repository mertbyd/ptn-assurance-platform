namespace Ptn.TestModule.Constants.Runs;

// islevi: Terminal hukum satirinin metin, json ve indeks sinirlarini tanimlar.
// sistemdeki gorevi: Manager boyut kapilari ile test_run_results EF eslemesini ayni DBML sozlesmesine baglar.
/// <summary>
/// Test kosum sonucu alanlarinin kararli sinirlarini ve indeks adlarini tasir.
/// </summary>
public static class TestRunResultConsts
{
    /// <summary>Makine hata kodunun azami karakter sayisidir.</summary>
    public const int MaxErrorCodeLength = 128;

    /// <summary>RFC 9457 detail metninin azami karakter sayisidir.</summary>
    public const int MaxDetailLength = 4000;

    /// <summary>Basarisiz adim adinin azami karakter sayisidir.</summary>
    public const int MaxStepNameLength = 256;

    /// <summary>Basarisiz adim yolunun azami karakter sayisidir.</summary>
    public const int MaxStepPathLength = 1000;

    /// <summary>Izlenen dal yolunun azami karakter sayisidir.</summary>
    public const int MaxBranchPathLength = 256;

    /// <summary>Satir icinde tutulabilecek diagnosis JSON boyutudur.</summary>
    public const int MaxDiagnosisReportBytes = 4096;

    /// <summary>Kosum ve deneme ciftinin unique indeks adidir.</summary>
    public const string AttemptIndexName = "ux_results_attempt";

    /// <summary>Makine hata kodu sorgularinin indeks adidir.</summary>
    public const string ErrorIndexName = "ix_results_error";
}
