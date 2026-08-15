namespace Ptn.TestModule.Constants.Runs;

// islevi: SARIF 2.1.0 belgesinin kararli alan adlarini, seviye degerlerini ve arac kimligini tanimlar.
// sistemdeki gorevi: Kod tarama yuzeyinin sozlesmesini tek Domain.Shared sahibinde tutar (PLAN-0003 TM-30).
/// <summary>
/// SARIF ihracatinin kararli alan ve seviye sabitlerini tasir.
/// </summary>
public static class SarifReportConsts
{
    /// <summary>Belgenin uyguladigi SARIF surumudur.</summary>
    public const string Version = "2.1.0";

    /// <summary>Belgenin dogrulandigi resmi SARIF sema adresidir.</summary>
    public const string SchemaUri = "https://json.schemastore.org/sarif-2.1.0.json";

    /// <summary>Bulgulari ureten aracin SARIF icindeki adidir.</summary>
    public const string ToolName = "ptn-assurance-platform";

    /// <summary>Parmak izi degerinin SARIF partialFingerprints anahtaridir.</summary>
    public const string FingerprintKey = "ptnFindingFingerprint/v1";

    // islevi: SARIF belgesinin adresledigi JSON alan adlarini merkezilestirir.
    /// <summary>SARIF belgesinin kararli JSON alan adlarini tasir.</summary>
    public static class Fields
    {
        public const string Schema = "$schema";
        public const string Version = "version";
        public const string Runs = "runs";
        public const string Tool = "tool";
        public const string Driver = "driver";
        public const string Name = "name";
        public const string Rules = "rules";
        public const string Id = "id";
        public const string Results = "results";
        public const string RuleId = "ruleId";
        public const string Level = "level";
        public const string Message = "message";
        public const string Text = "text";
        public const string Locations = "locations";
        public const string PhysicalLocation = "physicalLocation";
        public const string ArtifactLocation = "artifactLocation";
        public const string Uri = "uri";
        public const string PartialFingerprints = "partialFingerprints";
        public const string Properties = "properties";
        public const string OutcomeCode = "outcomeCode";
        public const string SourceCheckerCode = "sourceCheckerCode";
        public const string ComparisonKindCode = "comparisonKindCode";
        public const string RuleRef = "ruleRef";
        public const string Attempt = "attempt";
        public const string ExpectedValue = "expectedValue";
        public const string ObservedValue = "observedValue";
    }

    // islevi: SARIF'in kabul ettigi kapali seviye degerlerini merkezilestirir.
    /// <summary>SARIF sonuc seviyesi degerlerini tasir.</summary>
    public static class Level
    {
        public const string Error = "error";
        public const string Warning = "warning";
        public const string Note = "note";
        public const string None = "none";
    }
}
