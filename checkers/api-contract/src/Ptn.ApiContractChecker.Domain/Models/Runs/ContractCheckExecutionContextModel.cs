namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: UOW disindaki parse, normalize ve comparison icin gereken donmus run fotografini tasir.
// sistemdeki gorevi: Execute adimini repository ve degisebilir source tanimlarindan tamamen bagimsiz tutar.
public class ContractCheckExecutionContextModel
{
    // Sonucu yazilacak run kimligi.
    public Guid RunId { get; set; }

    // Referans snapshot'tan BuildContext aninda alinan ham UTF-8 govde.
    public byte[] BaseContent { get; set; } = [];

    // Aday snapshot'tan BuildContext aninda alinan ham UTF-8 govde.
    public byte[] TargetContent { get; set; } = [];

    // Yalniz bu calistirmaya ait gecici kapsam kurallari.
    public List<ContractCheckScopeRuleModel> ScopeRules { get; set; } = new();

    // x-internal true yuzeylerinin comparison disinda tutulup tutulmayacagi.
    public bool IgnoreInternal { get; set; }
}
