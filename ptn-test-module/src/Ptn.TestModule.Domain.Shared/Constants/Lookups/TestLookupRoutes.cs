namespace Ptn.TestModule.Constants.Lookups;

// islevi: Bes test lookup'inin salt-okunur HTTP adreslerini ve ortak Swagger grubunu tek sahipte tanimlar.
// sistemdeki gorevi: Ajanin lookup kodlarini kaynak okumadan kesfetmesini saglayan rotalari controller metinlerinden ayirir.
/// <summary>Test lookup okuma HTTP rota sabitlerini tasir.</summary>
public static class TestLookupRoutes
{
    /// <summary>Kosum durumu lookup koleksiyonunun kok rotasidir.</summary>
    public const string RunStatuses = "api/test-module/lookups/run-statuses";

    /// <summary>Test hukmu lookup koleksiyonunun kok rotasidir.</summary>
    public const string OutcomeStatuses = "api/test-module/lookups/outcome-statuses";

    /// <summary>Bulgu kategorisi lookup koleksiyonunun kok rotasidir.</summary>
    public const string FailureCategories = "api/test-module/lookups/failure-categories";

    /// <summary>Tetikleme turu lookup koleksiyonunun kok rotasidir.</summary>
    public const string TriggerKinds = "api/test-module/lookups/trigger-kinds";

    /// <summary>Senaryo yayin durumu lookup koleksiyonunun kok rotasidir.</summary>
    public const string ScenarioStates = "api/test-module/lookups/scenario-states";

    /// <summary>Kimlige gore tek lookup satiri rotasidir.</summary>
    public const string ById = "{id:guid}";

    /// <summary>Test lookup endpoint'lerinin Swagger grup adidir.</summary>
    public const string SwaggerGroupName = "test-module-lookups";
}
