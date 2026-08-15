namespace Ptn.TestModule.Constants.Runs;

// islevi: Kosum telemetrisinin ActivitySource, Meter, enstruman ve oznitelik adlarini tanimlar.
// sistemdeki gorevi: OTel semantic convention adlarini aynen tasiyan tek Domain.Shared sahibidir; ham log deposu acilmaz (PLAN-0003 TM-16 §2.6).
/// <summary>
/// Kosum telemetrisinin kararli kaynak, enstruman ve oznitelik sabitlerini tasir.
/// </summary>
public static class RunTelemetryConsts
{
    /// <summary>Kosum span'lerini yayan ActivitySource adidir.</summary>
    public const string ActivitySourceName = "Ptn.TestModule.Runs";

    /// <summary>Kosum olcumlerini yayan Meter adidir.</summary>
    public const string MeterName = "Ptn.TestModule.Runs";

    // islevi: OTel semantic convention enstruman adlarini merkezilestirir.
    /// <summary>Kosum olcum enstrumanlarinin kararli adlarini tasir.</summary>
    public static class Instruments
    {
        /// <summary>Tek bir test durumunun OTel semantic convention sayacidir.</summary>
        public const string TestCaseResultStatus = "test.case.result.status";

        /// <summary>Tum kosumun OTel semantic convention sayacidir.</summary>
        public const string TestSuiteRunStatus = "test.suite.run.status";
    }

    // islevi: Span ve olcum ozniteliklerinin adlarini merkezilestirir.
    /// <summary>Kosum telemetrisinin kararli oznitelik adlarini tasir.</summary>
    public static class Attributes
    {
        public const string TestKey = "test.case.name";
        public const string RunId = "test.run.id";
        public const string EnvironmentKey = "test.environment";
        public const string OutcomeCode = "test.case.result.status";
        public const string RunStatusCode = "test.suite.run.status";
        public const string FailureCategoryCode = "test.case.failure.category";
        public const string IsDryRun = "test.run.dry_run";
    }

    // islevi: Kosum span adlarini merkezilestirir.
    /// <summary>Kosum span adlarinin kararli degerlerini tasir.</summary>
    public static class Spans
    {
        /// <summary>Bir kosumun uctan uca icra span adidir.</summary>
        public const string Execute = "test_run.execute";

        /// <summary>Bir kosumun hukum cozumleme span adidir.</summary>
        public const string Judge = "test_run.judge";
    }
}
