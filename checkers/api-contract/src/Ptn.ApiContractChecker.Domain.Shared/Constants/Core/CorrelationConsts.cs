namespace Ptn.ApiContractChecker.Constants;

// islevi: Korelasyon trace ve adim anahtarinin kararli sekil sinirlarini tanimlar.
// sistemdeki gorevi: Iki checker ve tum public giris validator'lari icin ayni tel sozlesmesini korur.
public static class CorrelationConsts
{
    public const int TraceIdLength = 32;
    public const int MaxStepKeyLength = 128;
    public const string TraceIdPattern = "^[0-9a-f]{32}$";
}
