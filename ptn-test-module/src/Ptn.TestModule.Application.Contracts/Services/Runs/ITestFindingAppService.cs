using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Runs;

// islevi: Kosumlar arasi sayfali bulgu sorgusunu tanimlar.
// sistemdeki gorevi: Finding read modelini persistence entity'sini acmadan public yuzeye tasir.
public interface ITestFindingAppService : IApplicationService
{
    Task<PagedResultDto<TestFindingHeaderDto>> GetListAsync(TestFindingListInput input);
}
