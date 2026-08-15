using Ptn.TestModule.Dtos.Bridge.Diagnosis;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Bridge;

// islevi: API ve database basarisizliklarini ortak diagnosis kontratinda teshis eder.
// sistemdeki gorevi: Iki checker ailesini tek Application.Contracts servis yuzeyinde birlestirir.
public interface IFailureDiagnosisAppService : IApplicationService
{
    // API basarisizlik sinyalini normalize diagnosis raporuna cevirir.
    Task<DiagnosisReportDto> DiagnoseApiAsync(DiagnosisRequestDto input, CancellationToken cancellationToken);

    // Database basarisizlik sinyalini normalize diagnosis raporuna cevirir.
    Task<DiagnosisReportDto> DiagnoseDatabaseAsync(DiagnosisRequestDto input, CancellationToken cancellationToken);
}
