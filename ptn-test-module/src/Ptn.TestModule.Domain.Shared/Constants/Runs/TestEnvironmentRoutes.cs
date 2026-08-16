namespace Ptn.TestModule.Constants.Runs;

// islevi: Ortam baglama ve sandbox reset rotalarini tanimlar.
// sistemdeki gorevi: Ortam HTTP sozlesmesini Domain.Shared sahibinde tutar.
public static class TestEnvironmentRoutes
{
    public const string Root = "api/test-module/environments";
    public const string ByKey = "{key}";
    public const string SandboxReset = "{key}/sandbox/reset";
    public const string SwaggerGroupName = "test-module-environments";
}
