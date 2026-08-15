namespace Ptn.TestModule.Models.Catalog;

// islevi: Bulgu kayitlarindan okunan tek kural referansinin kac senaryoda gorundugunu tasir.
// sistemdeki gorevi: Kapsamin kural bazindaki payidir; kural envanteri de bu tarafta bilinmez.
public class ScenarioCoverageRuleGroup
{
    public string RuleRef { get; set; } = string.Empty;
    public int ScenarioCount { get; set; }
    public int FindingCount { get; set; }
}
