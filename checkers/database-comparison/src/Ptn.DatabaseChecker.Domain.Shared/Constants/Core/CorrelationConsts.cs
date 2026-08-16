namespace Ptn.DatabaseChecker.Constants;

// islevi: Checker cagrilarinda tasinan trace ve adim kimliklerinin kararli format sinirlarini tanimlar.
// sistemdeki gorevi: Iki checker'in public korelasyon sozlesmesi ile FluentValidation kurallarini ayni sabitlerde hizalar.
public static class CorrelationConsts
{
    public const int TraceIdLength = 32;
    public const int MaxStepKeyLength = 128;
    public const string TraceIdPattern = "^[0-9a-f]{32}$";
}
