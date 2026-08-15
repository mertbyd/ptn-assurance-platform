using System;
using System.Threading.Tasks;
using Ptn.TestModule.Dtos.Runs;
using Volo.Abp.Application.Services;

namespace Ptn.TestModule.Services.Runs;

// islevi: Bir kosumu standart rapor formatlarina ihrac etme use-case'ini tanimlar.
// sistemdeki gorevi: Agir ciktiyi blob deposuna yazip cagirana yalniz resource_link donen public sozlesmedir (PLAN-0003 TM-14/TM-30).
/// <summary>Kosum ihracat use-case'inin Application sozlesmesidir.</summary>
public interface IRunReportExportService : IApplicationService
{
    /// <summary>Kosumu tum standart formatlara ihrac edip artefakt baglarini getirir.</summary>
    /// <param name="id">Ihrac edilecek TestRun aggregate kimligi.</param>
    /// <returns>Uretilen ihracat artefaktlarinin resource_link gorunumu.</returns>
    Task<RunArtifactLinksDto> ExportAsync(Guid id);
}
