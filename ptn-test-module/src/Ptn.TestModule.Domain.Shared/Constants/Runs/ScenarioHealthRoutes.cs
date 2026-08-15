namespace Ptn.TestModule.Constants.Runs;

// islevi: Senaryo saglik okuma endpoint adreslerini ve Swagger grubunu tek sahipte tanimlar.
// sistemdeki gorevi: Saglik rota metinlerinin transport katmanina dagilmasini engeller.
/// <summary>Senaryo saglik HTTP rota sabitlerini tasir.</summary>
public static class ScenarioHealthRoutes
{
    /// <summary>Senaryo saglik koleksiyonunun kok rotasidir.</summary>
    public const string Root = "api/test-module/scenario-health";

    /// <summary>Tek senaryo anahtarinin saglik rotasidir.</summary>
    public const string ByScenarioKey = "{scenarioKey}";

    /// <summary>Senaryo saglik endpoint'lerinin Swagger grup adidir.</summary>
    public const string SwaggerGroupName = "test-module-scenario-health";
}
