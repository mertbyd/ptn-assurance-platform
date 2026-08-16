namespace Ptn.ApiContractChecker.ExceptionCodes.Diagnosis;

// islevi: Teshis ayar, probe ve guvenli hedef ihlallerinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Guvenlik ve configuration redlerini lokalize mesaj metninden ayirir.
public static class DiagnosisExceptionCodes
{
    public const string ExtractorNotFound = "ApiContractChecker.Diagnosis:ExtractorNotFound";
    public const string ProbeNotFound = "ApiContractChecker.Diagnosis:ProbeNotFound";
    public const string UnsafeProbeTarget = "ApiContractChecker.Diagnosis:UnsafeProbeTarget";
    public const string InvalidSetting = "ApiContractChecker.Diagnosis:InvalidSetting";
    public const string SnapshotIdRequired = "ApiContractChecker.Diagnosis:Validation:SnapshotIdRequired";
    public const string SignalRequired = "ApiContractChecker.Diagnosis:Validation:SignalRequired";
    public const string HttpMethodRequired = "ApiContractChecker.Diagnosis:Validation:HttpMethodRequired";
    public const string RequestPathRequired = "ApiContractChecker.Diagnosis:Validation:RequestPathRequired";
    public const string StatusCodeInvalid = "ApiContractChecker.Diagnosis:Validation:StatusCodeInvalid";
    public const string ProblemErrorInvalid = "ApiContractChecker.Diagnosis:Validation:ProblemErrorInvalid";
}
