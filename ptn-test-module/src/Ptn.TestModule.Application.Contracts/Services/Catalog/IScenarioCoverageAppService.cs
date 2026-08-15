using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Catalog;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Catalog;

// islevi: Senaryo kapsam raporu okuma use-case'ini tanimlar.
// sistemdeki gorevi: Derlenmis belgeyi acmadan Application katmaninin salt-okunur kapsam sozlesmesini sunar.
public interface IScenarioCoverageAppService : IApplicationService
{
    /// <summary>Yayinlanmis senaryolarin dokundugu operasyon ve kural kumelerini getirir.</summary>
    Task<ScenarioCoverageReportDto> GetCoverageAsync();
}
