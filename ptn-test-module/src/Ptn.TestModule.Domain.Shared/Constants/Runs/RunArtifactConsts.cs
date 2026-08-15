namespace Ptn.TestModule.Constants.Runs;

// islevi: Ihracat artefaktlarinin BLOB container adini, blob adi bicimini, dosya adlarini ve boyut butcesini tanimlar.
// sistemdeki gorevi: Agir ciktiyi satirdan cikarip yalniz resource_link birakan ihracat sozlesmesini tek Domain.Shared sahibinde tutar (PLAN-0003 TM-13, ADR-0016 §H).
/// <summary>
/// Kosum ihracat artefaktlarinin kararli container, ad ve boyut sabitlerini tasir.
/// </summary>
public static class RunArtifactConsts
{
    /// <summary>Ihracat artefaktlarinin yazildigi ABP BLOB Storing container adidir.</summary>
    public const string ContainerName = "test-module-run-artifacts";

    /// <summary>Blob adinin tenant, kosum, deneme ve format dosyasindan turetildigi bicimdir.</summary>
    public const string BlobNameFormat = "{0}/{1}/{2}/{3}";

    /// <summary>Blob adinin satirda tutulabilecegi azami karakter sayisidir.</summary>
    public const int MaxBlobNameLength = 256;

    /// <summary>Bir ihracat artefaktinin uretebilecegi azami bayt sayisidir.</summary>
    public const int MaxArtifactBytes = 33_554_432;

    /// <summary>JUnit bag kolonunun kararli adidir; snake_case convention'i "j_unit_blob_name" uretir.</summary>
    public const string JUnitBlobColumnName = "junit_blob_name";

    // islevi: Uc ihracat formatinin kararli blob dosya adlarini merkezilestirir.
    /// <summary>Ihracat formatlarinin blob adindaki dosya bolumlerini tasir.</summary>
    public static class FileNames
    {
        /// <summary>CTRF raporunun blob dosya adidir.</summary>
        public const string Ctrf = "ctrf.json";

        /// <summary>JUnit raporunun blob dosya adidir.</summary>
        public const string JUnit = "junit.xml";

        /// <summary>SARIF raporunun blob dosya adidir.</summary>
        public const string Sarif = "sarif.json";
    }
}
