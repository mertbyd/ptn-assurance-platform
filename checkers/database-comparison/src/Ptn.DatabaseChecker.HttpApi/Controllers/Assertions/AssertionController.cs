using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Assertions;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Assertions;

// islevi: Test Module row, count, absent ve batch assertion endpoint'lerini HTTP uzerinden acar.
// sistemdeki gorevi: Named permission ile korunan ince transport wrapper'idir; tum kararlar AppService ve tek manager cekirdegindedir.
/// <summary>
/// Hedefli veritabani assertion islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.Assertions)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Assertions)]
[Authorize(DatabaseCheckerPermissions.Assertions.Default)]
public class AssertionController : DatabaseCheckerController
{
    private IDatabaseAssertionAppService AppService
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseAssertionAppService>();

    private IAssertionDerivabilityAppService DerivabilityAppService
        => LazyServiceProvider.LazyGetRequiredService<IAssertionDerivabilityAppService>();

    /// <summary>
    /// Anahtarla secilen tek satirin kolon beklentilerini dogrular.
    /// </summary>
    /// <param name="input">Baglanti, tablo, anahtar ve kolon beklentileri.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde hedef okumayi durduran token.</param>
    /// <returns>Assertion sonucu ve guvenli hata kaniti.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.RowAssertion)]
    [Authorize(DatabaseCheckerPermissions.Assertions.Execute)]
    public async Task<Result<RowAssertionResultDto>> AssertRow(
        [FromBody] RowAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.AssertRowAsync(input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Anahtarla secilen satir kumesinin cardinality beklentisini dogrular.
    /// </summary>
    /// <param name="input">Baglanti, tablo, anahtar ve cardinality beklentisi.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde hedef okumayi durduran token.</param>
    /// <returns>Gozlenen satir sayisini iceren assertion sonucu.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.CountAssertion)]
    [Authorize(DatabaseCheckerPermissions.Assertions.Execute)]
    public async Task<Result<RowAssertionResultDto>> AssertCount(
        [FromBody] RowAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.AssertCountAsync(input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Anahtarla secilen satirin bulunmadigini dogrular.
    /// </summary>
    /// <param name="input">Baglanti, tablo ve bulunmamasi gereken satirin anahtari.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde hedef okumayi durduran token.</param>
    /// <returns>Yokluk assertion sonucu.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.AbsentAssertion)]
    [Authorize(DatabaseCheckerPermissions.Assertions.Execute)]
    public async Task<Result<RowAssertionResultDto>> AssertAbsent(
        [FromBody] RowAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.AssertAbsentAsync(input, cancellationToken);
        return result;
    }
    /// <summary>
    /// Coklu assertion'i tek HTTP/MCP round-trip'te calistirir.
    /// </summary>
    /// <param name="input">Bagimsiz assertion istekleri.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde batch okumalarini durduran token.</param>
    /// <returns>Girdi sirasi korunmus assertion sonuclari.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.BatchAssertion)]
    [Authorize(DatabaseCheckerPermissions.Assertions.Execute)]
    public async Task<Result<List<RowAssertionResultDto>>> AssertBatch(
        [FromBody] List<RowAssertionRequestDto> input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.AssertBatchAsync(input, cancellationToken);
        return result;
    }
    /// <summary>
    /// DB assertion adreslerini tablo, kolon, unique anahtar ve matcher-tip kapilarinda dogrular.
    /// </summary>
    /// <param name="input">Baglanti ve turetilebilirligi sinanacak assertion adresleri.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde katalog okumasini durduran token.</param>
    /// <returns>Her assertion icin tablo/kolon referansi ve tek kapali outcome.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.AssertionDerivability)]
    [Authorize(DatabaseCheckerPermissions.Assertions.ValidateDerivability)]
    public async Task<Result<DerivabilityResultDto>> ValidateDerivability(
        [FromBody] DerivabilityRequestDto input,
        CancellationToken cancellationToken)
    {
        var result = await DerivabilityAppService.ValidateDerivabilityAsync(input, cancellationToken);
        return result;
    }
}
