namespace Ptn.TestModule.Constants.Bridge;

// islevi: Checker korelasyon kimliginin kararli trace ve adim sinirlarini tanimlar.
// sistemdeki gorevi: Public DTO validator'lariyla iki checker tel sozlesmesini ayni gramerde tutar.
public static class PtnCorrelationConsts
{
    public const int TraceIdLength = 32;
    public const int MaxStepKeyLength = 128;
    public const string TraceIdPattern = "^[0-9a-f]{32}$";
    public const string WriteSetCaptureStepKey = "write-set-capture";
}
