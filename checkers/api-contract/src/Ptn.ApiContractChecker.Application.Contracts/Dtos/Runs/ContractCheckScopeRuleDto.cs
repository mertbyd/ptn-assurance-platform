namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Tek bir calistirmaya ait OpenAPI kapsam filtresini API sinirinda tasir.
// sistemdeki gorevi: Include/exclude desenini job payload'ina aktarir ve ContractCheckRun tablosuna yazilmasini engeller.
public class ContractCheckScopeRuleDto
{
    // Kuralin include veya exclude kararli kodu.
    public string KindCode { get; set; } = default!;

    // Desenin path, tag, operation-id veya schema hedefi.
    public string TargetCode { get; set; } = default!;

    // Basit wildcard destekleyen gecici eslesme deseni.
    public string Pattern { get; set; } = default!;
}
