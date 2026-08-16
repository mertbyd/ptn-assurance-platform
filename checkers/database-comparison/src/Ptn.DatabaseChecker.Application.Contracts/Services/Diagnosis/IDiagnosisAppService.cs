using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services.Diagnosis;

// islevi: Test Module icin tek dinamik failure diagnosis use-case'ini tanimlar.
// sistemdeki gorevi: Assertion veya provider sinyalini ayni kalici-olmayan RFC 9457 raporuna goturen public application sozlesmesidir.
public interface IDiagnosisAppService : IApplicationService
{
    // islevi: Tek failure sinyalini katalog-resolved sirali hipotez raporuna cevirir.
    Task<DiagnosisReportDto> DiagnoseAsync(
        DiagnoseRequestDto input,
        CancellationToken cancellationToken = default);
}
