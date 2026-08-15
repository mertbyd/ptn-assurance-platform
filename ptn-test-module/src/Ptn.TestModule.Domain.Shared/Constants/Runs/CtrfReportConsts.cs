namespace Ptn.TestModule.Constants.Runs;

// islevi: CTRF belgesinin kararli alan adlarini, durum degerlerini ve arac kimligini tanimlar.
// sistemdeki gorevi: Ihracat sozlesmesini tek Domain.Shared sahibinde tutar; hukum eslemesi kayipsizdir (PLAN-0003 TM-14 §2.2).
/// <summary>
/// CTRF ihracatinin kararli alan ve durum sabitlerini tasir.
/// </summary>
public static class CtrfReportConsts
{
    /// <summary>Raporu ureten aracin CTRF icindeki adidir.</summary>
    public const string ToolName = "ptn-assurance-platform";

    // islevi: CTRF belgesinin adresledigi JSON alan adlarini merkezilestirir.
    /// <summary>CTRF belgesinin kararli JSON alan adlarini tasir.</summary>
    public static class Fields
    {
        public const string Results = "results";
        public const string Tool = "tool";
        public const string Name = "name";
        public const string Summary = "summary";
        public const string Tests = "tests";
        public const string Passed = "passed";
        public const string Failed = "failed";
        public const string Pending = "pending";
        public const string Skipped = "skipped";
        public const string Other = "other";
        public const string Start = "start";
        public const string Stop = "stop";
        public const string Status = "status";
        public const string Duration = "duration";
        public const string Message = "message";
        public const string Environment = "environment";
        public const string TestEnvironment = "testEnvironment";
        public const string Extra = "extra";
        public const string OutcomeCode = "outcomeCode";
        public const string Attempt = "attempt";
        public const string ErrorCode = "errorCode";
        public const string FailedStepName = "failedStepName";
        public const string FailedStepOrdinal = "failedStepOrdinal";
        public const string TraceId = "traceId";
        public const string RunnerRef = "runnerRef";
        public const string SpecFingerprint = "specFingerprint";
        public const string DbSchemaFingerprint = "dbSchemaFingerprint";
        public const string IsDryRun = "isDryRun";
    }

    // islevi: CTRF'nin kabul ettigi kapali durum degerlerini merkezilestirir.
    /// <summary>CTRF test durumu degerlerini tasir.</summary>
    public static class Status
    {
        public const string Passed = "passed";
        public const string Failed = "failed";
        public const string Skipped = "skipped";
        public const string Pending = "pending";
        public const string Other = "other";
    }
}
