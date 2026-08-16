namespace Ptn.ApiContractChecker.Constants.Diagnostics;

// islevi: Checker ActivitySource span ve guvenli attribute adlarini tanimlar.
// sistemdeki gorevi: Govde, token, host ve query degeri tasimayan ortak gozlemlenebilirlik sozlesmesidir.
public static class ApiContractCheckerDiagnostics
{
    public const string ActivitySourceName = "CheckNexus.ApiContracts";
    public const string ConformanceAssertSpan = "checknexus.api.conformance.assert";
    public const string DiagnosisRunSpan = "checknexus.api.diagnosis.run";
    public const string DiagnosisProbeSpan = "checknexus.api.diagnosis.probe";
    public const string FindingsPageSpan = "checknexus.api.findings.page";
    public const string SnapshotDescribeSpan = "checknexus.api.snapshot.describe";
    public const string RunIdAttribute = "checknexus.run.id";
    public const string MomentAttribute = "checknexus.moment";
    public const string ResponseBytesAttribute = "checknexus.response_bytes";
    public const string ErrorTypeAttribute = "error.type";
    public const string MomentAuthoring = "A";
    public const string MomentRuntime = "B";
    public const string MomentDiagnosis = "C";
    public const string MomentMaintenance = "D";
    public const string TimeoutErrorType = "timeout";
    public const string ProbeErrorType = "probe";
}
