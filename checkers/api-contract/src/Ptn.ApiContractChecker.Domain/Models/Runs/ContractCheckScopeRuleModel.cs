namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Tek calistirmaya ait include/exclude OpenAPI kapsam kuralini domaine tasir.
// sistemdeki gorevi: Application DTO'sunu comparison filtresinden ayirir ve kalici modele girmeyen runtime kuralini temsil eder.
public class ContractCheckScopeRuleModel
{
    // Include veya exclude kararli kodu.
    public string KindCode { get; set; } = default!;

    // Path, tag, operation-id veya schema kararli hedef kodu.
    public string TargetCode { get; set; } = default!;

    // Basit wildcard ile eslestirilecek kapsam deseni.
    public string Pattern { get; set; } = default!;
}
