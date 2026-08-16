namespace Ptn.ApiContractChecker.ExceptionCodes.Conformance;

// islevi: Conformance ayar ve girdi ihlallerinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: FluentValidation ve policy resolver hatalarini mesaj metninden bagimsiz ABP hata sozlesmesine baglar.
public static class ConformanceExceptionCodes
{
    public const string SnapshotIdRequired = "ApiContractChecker.Conformance:Validation:SnapshotIdRequired";
    public const string HttpMethodRequired = "ApiContractChecker.Conformance:Validation:HttpMethodRequired";
    public const string RequestPathRequired = "ApiContractChecker.Conformance:Validation:RequestPathRequired";
    public const string SampleKindInvalid = "ApiContractChecker.Conformance:Validation:SampleKindInvalid";
    public const string MaxSamplesPerFieldInvalid = "ApiContractChecker.Conformance:Validation:MaxSamplesPerFieldInvalid";
    public const string SourceOperationIdRequired = "ApiContractChecker.Conformance:Validation:SourceOperationIdRequired";
    public const string MaxCandidatesInvalid = "ApiContractChecker.Conformance:Validation:MaxCandidatesInvalid";
    public const string StatusCodeInvalid = "ApiContractChecker.Conformance:Validation:StatusCodeInvalid";
    public const string ProfileInvalid = "ApiContractChecker.Conformance:Validation:ProfileInvalid";
    public const string RetentionModeInvalid = "ApiContractChecker.Conformance:RetentionModeInvalid";
    public const string RedactionSaltMissing = "ApiContractChecker.Conformance:RedactionSaltMissing";
    public const string SettingsInvalid = "ApiContractChecker.Conformance:SettingsInvalid";
}
