using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Volo.Abp.Application.Services;

namespace Ptn.ApiContractChecker.Services.Diagnosis;

// islevi: Basarisiz API adimi icin tek deterministik diagnosis use-case kontratini tanimlar.
// sistemdeki gorevi: HTTP ve composition host yuzeylerini Application implementasyonundan ayirir.
public interface IDiagnosisAppService : IApplicationService
{
    Task<DiagnosisReportDto> DiagnoseAsync(DiagnoseRequestDto input);
}
