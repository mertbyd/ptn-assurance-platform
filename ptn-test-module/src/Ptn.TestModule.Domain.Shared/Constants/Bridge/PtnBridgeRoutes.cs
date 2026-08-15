namespace Ptn.TestModule.Constants.Bridge;

// islevi: Bridge HTTP ve agir-govde resource adreslerini tek sahipte tanimlar.
// sistemdeki gorevi: Controller, Swagger ve response resource linklerinin ayni kararli rotalari kullanmasini saglar.
public static class PtnBridgeRoutes
{
    public const string Root = "api/test-module/bridge";
    public const string Ground = "ground";
    public const string Explain = "explain";
    public const string Validate = "validate";
    public const string Knowledge = "knowledge";
    public const string ToolCatalog = "tools";
    public const string AgentProfile = "agent-profile";
    public const string ToolBudget = "tool-budget";
    public const string TaskStatus = "task-status";
    public const string OverlaySuggestion = "overlay-suggestion";
    public const string Resources = Root + "/resources";
    public const string SwaggerGroupName = "test-module-bridge";

    public static string Resource(string toolCode) => $"/{Resources}/{toolCode}";
}
