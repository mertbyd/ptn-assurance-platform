using Ptn.DatabaseChecker.Dtos.Projections;
using Volo.Abp.Application.Services;

namespace Ptn.DatabaseChecker.Services.Projections;

// islevi: Katalog-adresli, redaksiyonlu ve sinirli satir projection use-case'ini tanimlar.
// sistemdeki gorevi: HTTP katmanini concrete Application servisinden ayiran public ABP kontratidir.
public interface IProjectionAppService : IApplicationService
{
    // islevi: Anahtarla secilen satirlarin yalniz istenen kolonlarini salt-okunur okur.
    Task<ProjectionResultDto> ProjectRowsAsync(
        ProjectionRequestDto input,
        CancellationToken cancellationToken = default);
}
