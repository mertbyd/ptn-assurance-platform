namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Iki mevcut snapshot arasinda asenkron contract check baslatma istegini tasir.
// sistemdeki gorevi: Istemciden durum almadan snapshot kimlikleriyle yalniz bu calistirmaya ait kapsam kurallarini kabul eder.
public class ExecuteContractCheckDto
{
    // Karsilastirmanin referans snapshot kimligi.
    public Guid BaseSnapshotId { get; set; }

    // Karsilastirmanin aday snapshot kimligi.
    public Guid TargetSnapshotId { get; set; }

    // Calistirma tamamlaninca olen, tabloya yazilmayan kapsam kurallari.
    public List<ContractCheckScopeRuleDto>? ScopeRules { get; set; }

    // x-internal true tasiyan operasyon ve semalari bu calistirmadan dislar.
    public bool IgnoreInternal { get; set; }
}
