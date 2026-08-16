using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Projections;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Projections;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Projections;

// islevi: Redaksiyonlu ve sinirli salt-okunur projection endpointini HTTP uzerinden acar.
// sistemdeki gorevi: Named permission ile korunan ince transport wrapper'idir; tum kararlar AppService ve Manager'dadir.
/// <summary>
/// Hedefli salt-okunur veritabani projection islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.Projections)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Projections)]
[Authorize(DatabaseCheckerPermissions.Projections.Execute)]
public class ProjectionController : DatabaseCheckerController
{
    private IProjectionAppService AppService
        => LazyServiceProvider.LazyGetRequiredService<IProjectionAppService>();

    /// <summary>
    /// Katalogda dogrulanmis anahtarla eslesen satirlarin yalniz secili kolonlarini redaksiyonlu okur.
    /// </summary>
    /// <param name="input">Baglanti, tablo, anahtar, projection kolonlari ve satir butcesi.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde hedef okumayi durduran token.</param>
    /// <returns>Outcome, redaksiyonlu satirlar ve acik truncation bilgisi.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.ProjectionRows)]
    public async Task<Result<ProjectionResultDto>> ProjectRows(
        [FromBody] ProjectionRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.ProjectRowsAsync(input, cancellationToken);
        return result;
    }
}
