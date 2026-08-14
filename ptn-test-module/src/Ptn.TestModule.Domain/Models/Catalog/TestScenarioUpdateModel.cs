namespace Ptn.TestModule.Models.Catalog;

// islevi: Draft veya onay bekleyen senaryo surumunun degistirilebilir alanlarini tasir.
// sistemdeki gorevi: Kararli ScenarioKey ve VersionNo alanlarini update yuzeyinin disinda tutar.
public sealed class TestScenarioUpdateModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceDocument { get; set; } = string.Empty;
    public string SourceHash { get; set; } = string.Empty;
    public string CompiledDocument { get; set; } = string.Empty;
    public string CompiledHash { get; set; } = string.Empty;
    public TestScenarioMaterialSeal MaterialSeal { get; set; } = new();
    public int AssertionCount { get; set; }
    public string? DerivabilityCode { get; set; }
    public bool AuthoredByAgent { get; set; }
    public string? AgentModelRef { get; set; }
    public string? Notes { get; set; }
}
