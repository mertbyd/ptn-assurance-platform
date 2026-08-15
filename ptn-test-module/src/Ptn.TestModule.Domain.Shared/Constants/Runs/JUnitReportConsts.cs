namespace Ptn.TestModule.Constants.Runs;

// islevi: JUnit XML belgesinin kararli element ve nitelik adlarini tanimlar.
// sistemdeki gorevi: Failed'in <failure>, Broken'in <error> karsiligini tek Domain.Shared sahibinde sabitler (PLAN-0003 TM-14 §2.2).
/// <summary>
/// JUnit ihracatinin kararli element ve nitelik sabitlerini tasir.
/// </summary>
public static class JUnitReportConsts
{
    /// <summary>Belgenin basina yazilan kararli XML bildirimidir.</summary>
    public const string XmlDeclaration = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

    // islevi: JUnit belgesinin element adlarini merkezilestirir.
    /// <summary>JUnit belgesinin kararli element adlarini tasir.</summary>
    public static class Elements
    {
        public const string TestSuites = "testsuites";
        public const string TestSuite = "testsuite";
        public const string TestCase = "testcase";
        public const string Failure = "failure";
        public const string Error = "error";
        public const string Skipped = "skipped";
    }

    // islevi: JUnit belgesinin nitelik adlarini merkezilestirir.
    /// <summary>JUnit belgesinin kararli nitelik adlarini tasir.</summary>
    public static class Attributes
    {
        public const string Name = "name";
        public const string ClassName = "classname";
        public const string Tests = "tests";
        public const string Failures = "failures";
        public const string Errors = "errors";
        public const string Skipped = "skipped";
        public const string Time = "time";
        public const string Message = "message";
        public const string Type = "type";
        public const string Hostname = "hostname";
    }
}
