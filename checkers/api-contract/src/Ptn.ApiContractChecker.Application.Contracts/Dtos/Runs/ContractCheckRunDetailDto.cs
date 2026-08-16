namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Tek contract check run basligini tam findings govdesiyle birlikte dondurur.
// sistemdeki gorevi: Agir bulgu satirlarini yalniz kimlikli detay endpointinde aciga cikarir.
public class ContractCheckRunDetailDto : ContractCheckRunHeaderDto
{
    public ContractCheckFindingsDto Findings { get; set; } = new();
}
