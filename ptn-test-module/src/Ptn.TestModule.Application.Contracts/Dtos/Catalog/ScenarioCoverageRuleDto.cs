namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Tek kural referansinin kac yayinlanmis senaryoda ve kac bulguda gorundugunu API cevabinda tasir.
// sistemdeki gorevi: Kapsamin kural bazindaki payidir; kural envanteri bu tarafta bilinmez.
/// <summary>Kural bazinda kapsam payinin public gorunumudur.</summary>
public class ScenarioCoverageRuleDto
{
    /// <summary>Bulgularda gorulen kararli kural referansidir.</summary>
    public string RuleRef { get; set; } = string.Empty;

    /// <summary>Bu kurala deger yayinlanmis senaryo sayisidir.</summary>
    public int ScenarioCount { get; set; }

    /// <summary>Bu kurala bagli toplam bulgu sayisidir.</summary>
    public int FindingCount { get; set; }
}
