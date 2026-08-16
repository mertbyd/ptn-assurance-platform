namespace Ptn.ApiContractChecker.Constants.Sources;

// islevi: SpecDocument alanlarinin kalici uzunluk sinirlarini tanimlar.
// sistemdeki gorevi: Aggregate davranisi ile EF kolonlarini tek sozlesmede hizalar.
public static class SpecDocumentConsts
{
    public const int MaxDocumentNameLength = 64;
    public const int MaxPathLength = 256;
    public const int MaxFetchOutcomeLength = 64;

    // Izlenen dokumanin iki kontrol arasinda bekleyecegi en kisa sure; daha sikisi izlenen servisi doverdi.
    public const int MinCheckIntervalMinutes = 1;

    // Izlenen dokumanin iki kontrol arasinda bekleyebilecegi en uzun sure (yedi gun).
    public const int MaxCheckIntervalMinutes = 10_080;
}
