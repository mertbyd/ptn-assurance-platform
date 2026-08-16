namespace Ptn.ApiContractChecker.Constants.Diagnosis;

// islevi: Olgu ve safe HTTP probe turleri ile yapilandirilmis kanit kodlarini tanimlar.
// sistemdeki gorevi: Rule-probe bagini sinif adi, URL veya serbest metinden bagimsiz tutar.
public static class ProbeKindCodes
{
    public const string SpecFact = "SpecFact";
    public const string SchemaViolationLocation = "SchemaViolationLocation";
    public const string ContractDriftFact = "ContractDriftFact";
    public const string ResponseHeaderFact = "ResponseHeaderFact";
    public const string SnapshotFreshness = "SnapshotFreshness";
    public const string OptionsAllow = "OptionsAllow";
    public const string HeadResource = "HeadResource";
    public const string ServerReachability = "ServerReachability";
    public const string AuthMetadata = "AuthMetadata";
    public const string HttpClientName = "ApiContractCheckerDiagnosis";

    public static class Facts
    {
        public const string Present = "Present";
        public const string Absent = "Absent";
        public const string Match = "Match";
        public const string Mismatch = "Mismatch";
        public const string Reachable = "Reachable";
        public const string Unreachable = "Unreachable";
        public const string TimedOut = "TimedOut";
    }

    public static class Names
    {
        public const string Operation = "operation";
        public const string RequestMediaType = "request-media-type";
        public const string SecurityScope = "security-scope";
        public const string ResponseVersion = "response-version";
    }
}
