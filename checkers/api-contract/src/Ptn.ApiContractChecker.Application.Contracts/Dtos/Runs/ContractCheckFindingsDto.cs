namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Bir contract check run'inin tum bulgu satirlarini API detay govdesinde tasir.
// sistemdeki gorevi: Domain owned JSON modelini dogrudan disariya acmadan istemci sozlesmesine cevirir.
public class ContractCheckFindingsDto
{
    public List<FindingDto> Items { get; set; } = [];
}
