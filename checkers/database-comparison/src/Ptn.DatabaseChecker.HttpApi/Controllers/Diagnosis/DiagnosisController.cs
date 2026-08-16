using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Diagnosis;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Diagnosis;

// islevi: Test Module dinamik failure diagnosis endpoint'ini HTTP uzerinden acar.
// sistemdeki gorevi: Diagnosis.Execute ile korunan tek ince transport wrapper'idir; tum kararlar AppService ve DiagnosisManager'dadir.
/// <summary>
/// Dinamik veritabani hata teshisi.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.Diagnosis)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Diagnosis)]
[Authorize(DatabaseCheckerPermissions.Diagnosis.Execute)]
public class DiagnosisController : DatabaseCheckerController
{
    private IDiagnosisAppService AppService
        => LazyServiceProvider.LazyGetRequiredService<IDiagnosisAppService>();

    /// <summary>
    /// Assertion veya yapilandirilmis database-exception sinyalini sirali hipotez raporuna cevirir.
    /// </summary>
    /// <param name="input">Baglanti kimligi ve yapilandirilmis hata sinyali.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde katalog ve probe okumalarini durduran token.</param>
    /// <returns>Kanita gore siralanmis teshis raporu.</returns>
    [HttpPost]
    public async Task<Result<DiagnosisReportDto>> Diagnose(
        [FromBody] DiagnoseRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.DiagnoseAsync(input, cancellationToken);
        return result;
    }
}
