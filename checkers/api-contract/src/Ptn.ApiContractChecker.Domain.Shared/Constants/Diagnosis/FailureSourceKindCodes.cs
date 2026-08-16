namespace Ptn.ApiContractChecker.Constants.Diagnosis;

using Ptn.ApiContractChecker.Constants;

// islevi: API teshis sinyal kaynaklarini ve RFC 9457 rapor sinirlarini tanimlar.
// sistemdeki gorevi: Domain raporu, HTTP rotasi ve transport butcesini tek kararli sozlesmede bulusturur.
public static class FailureSourceKindCodes
{
    public const string Conformance = "Conformance";
    public const string HttpStatus = "HttpStatus";
    public const string Transport = "Transport";

    public static class Report
    {
        public const string Type = "urn:checknexus:problem:api-contract-diagnosis";
        public const string Instance = ApiContractCheckerRoutes.Diagnosis;
        public const int Status = 200;
        public const int MaxUtf8Bytes = 4096;
        public const int MaxEvidencePerHypothesis = 3;
        public const int MaxNextChecks = 3;
        public const int SerializationMarginBytes = 256;
    }
}
