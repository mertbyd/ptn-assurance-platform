using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.Dtos.Runs.Reports;

namespace Ptn.ApiContractChecker.Services.Runs;

// islevi: Contract check tetikleme, gecmis, detay, hafif status ve deterministik rapor sozlesmelerini tanimlar.
// sistemdeki gorevi: Istemcinin durum yazmasini engeller; yalniz snapshot tabanli sistem tetiklemesini ve salt-okunur sonuc yuzeyini acar.
public interface IContractCheckRunAppService
    : IEntityReadAppService<ContractCheckRunDetailDto, ContractCheckRunHeaderDto, GetContractCheckRunsInput>
{
    // Iki mevcut snapshot icin Pending run olusturup comparison job'unu kuyruklar.
    Task<ContractCheckRunStatusDto> ExecuteAsync(ExecuteContractCheckDto input);

    // Run durumunu findings govdesini cekmeden getirir.
    Task<ContractCheckRunStatusDto> GetStatusAsync(Guid id);

    // Run findings govdesinden her istekte yeniden uretilen raporu getirir.
    Task<ContractCheckReportDto> GetReportAsync(Guid id);

    // Run bulgularini degisim durumu ve filtrelerle ABP sayfasi olarak getirir.
    Task<FindingPagedResultDto> GetFindingsAsync(Guid id, GetContractCheckFindingsInput input);
}
