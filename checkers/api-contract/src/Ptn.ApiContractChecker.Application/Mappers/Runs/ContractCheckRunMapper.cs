using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.Dtos.Runs.Reports;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Runs.Reports;
using Riok.Mapperly.Abstractions;

namespace Ptn.ApiContractChecker.Application.Mappers.Runs;

// islevi: Contract check scope girdisiyle baslik, detay, status, findings ve rapor modellerini katmanlar arasinda tasir.
// sistemdeki gorevi: Run tetikleme ve okuma yuzeyindeki tum katmanlar arasi alan tasimanin tek Mapperly sahibidir.
[Mapper]
public partial class ContractCheckRunMapper
{
    // Gecici scope DTO listesini tabloya dokunmayan domain execution modellerine tasir.
    public partial List<ContractCheckScopeRuleModel> MapToScopeRules(List<ContractCheckScopeRuleDto> rules);

    // Hafif run basligini liste DTO'suna tasir.
    public partial ContractCheckRunHeaderDto MapToHeaderDto(ContractCheckRunHeaderModel model);

    // Hafif run baslik listesini liste DTO'larina tasir.
    public partial List<ContractCheckRunHeaderDto> MapToHeaderDto(List<ContractCheckRunHeaderModel> models);

    // Tam run ve findings modelini detay DTO'suna tasir.
    public partial ContractCheckRunDetailDto MapToDetailDto(ContractCheckRunDetailModel model);

    // Hafif run basligini polling status DTO'suna tasir.
    public partial ContractCheckRunStatusDto MapToStatusDto(ContractCheckRunHeaderModel model);

    // Deterministik domain raporunu API rapor DTO'suna tasir.
    public partial ContractCheckReportDto MapToReportDto(ContractCheckReportAggregationModel model);

    // Siniflandirilmis ve butcelenmis domain bulgu sayfasini ABP sonuc sozlesmesine tasir.
    public partial FindingPagedResultDto MapToFindingPageDto(FindingPageResultModel model);

    // Detay endpointi change state hesaplamadigi icin yalniz o hedef alani bilincli olarak dislar.
    [MapperIgnoreTarget(nameof(FindingDto.ChangeStateCode))]
    private partial FindingDto MapToFindingDto(Finding model);
}
