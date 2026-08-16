namespace Ptn.TestModule.Constants.Authoring;

// islevi: Gecici yazarlik oturumunun cache, TTL ve public girdi butcelerini tanimlar.
// sistemdeki gorevi: Cache omrunu ve tek-adim sinirlarini kod ile testler arasinda sabitler.
public static class AuthoringSessionConsts
{
    public const string CacheName = "TestModuleAuthoringSessions";
    public const int TtlMinutes = 30;
    public const long TtlMilliseconds = TtlMinutes * 60L * 1_000L;
    public const int MaxWorkflowIdLength = 128;
    public const int MaxWorkflowSummaryLength = 512;
    public const int MaxSourceUrlLength = 2_048;
    public const int MaxStepIdLength = 128;
    public const int MaxRequestBodyBytes = 65_536;
    public const int MaxAssertionsPerStep = 32;
    public const string WorkflowIdPattern = "^[a-zA-Z0-9][a-zA-Z0-9._-]{0,127}$";
    public const string StepIdPattern = "^[a-zA-Z0-9][a-zA-Z0-9._-]{0,127}$";
    public const string JsonPointerPattern = "^/(?:[^~/]|~[01])*(?:/(?:[^~/]|~[01])*)*$";
}
