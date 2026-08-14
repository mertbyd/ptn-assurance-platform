namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Mevcut senaryo surumunun degistirilebilir public alanlarini tasir.
// sistemdeki gorevi: ScenarioKey ve VersionNo'yu update yuzeyinin disinda tutar.
public sealed class UpdateTestScenarioDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceDocument { get; set; } = string.Empty;
    public string SourceHash { get; set; } = string.Empty;
    public string CompiledDocument { get; set; } = string.Empty;
    public string CompiledHash { get; set; } = string.Empty;
    public TestScenarioMaterialSealDto MaterialSeal { get; set; } = new();
    public int AssertionCount { get; set; }
    public string? DerivabilityCode { get; set; }
    public bool AuthoredByAgent { get; set; }
    public string? AgentModelRef { get; set; }
    public string? Notes { get; set; }
}
