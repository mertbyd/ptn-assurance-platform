namespace Ptn.ApiContractChecker.Constants.Sources;

// islevi: SpecSource alanlarinin kalici uzunluk sinirlarini tanimlar.
// sistemdeki gorevi: Domain invariantlariyla EF kolon eslemesinin ayni sinirlari kullanmasini saglar.
public static class SpecSourceConsts
{
    public const int MaxNameLength = 128;
    public const int MaxBaseUrlLength = 512;
    public const int MaxVaultSecretPathLength = 256;
    public const int MaxHeaderNameLength = 128;
    public const int MaxHeaderValueLength = 4096;
    public const string HeaderNamePattern = "^[!#$%&'*+.^_`|~0-9A-Za-z-]+$";
    public const string HttpClientName = "ApiContractChecker.SpecSource";

    // Guvenilmeyen spec govdesi sinirli okunurken kullanilan tampon boyutu.
    public const int FetchReadBufferSizeBytes = 81_920;
}
