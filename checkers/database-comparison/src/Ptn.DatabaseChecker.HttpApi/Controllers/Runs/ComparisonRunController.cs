using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Reports;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.Dtos.Findings;
using Volo.Abp.Application.Dtos;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Runs;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Runs;

// islevi: Karsilastirma run okuma + calistir-ve-sakla + rapor endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: ComparisonRun liste/get header, findings/kapsam detay, bir tarifi calistirip kalici run uretme ve yapisal rapor isteklerini IComparisonRunAppService'e yonlendiren ince controller katmanidir. Okuma View, calistirma Create yetkisi ister; update/delete acilmaz.
/// <summary>
/// Karsilastirma run okuma, calistirma ve raporlama islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.ComparisonRuns)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Runs)]
[Authorize(DatabaseCheckerPermissions.Runs.View)]
public class ComparisonRunController : EntityReadControllerBase<IComparisonRunAppService, ComparisonRunDto>
{
    /// <summary>
    /// Run'i tum bulgular ve rapor icerikleriyle (agir owned veri) getirir; scope kurallari response'a dahil edilmez.
    /// </summary>
    /// <param name="id">Detayi okunacak run'in kimligi.</param>
    /// <returns>Run'in agir detay cevap modeli.</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.RunDetail)]
    public async Task<Result<ComparisonRunDetailDto>> GetDetail(Guid id)
    {
        var result = await AppService.GetDetailAsync(id);
        return result;
    }
    /// <summary>
    /// Bir tarifi fiilen calistirir ve sonucu kalici run olarak saklar.
    /// </summary>
    /// <param name="input">Calistirilacak tarifin kimligini tasiyan istek.</param>
    /// <returns>Olusan run'in agir detay cevap modeli.</returns>
    [HttpPost(DatabaseCheckerHttpApiConstants.Segments.RunExecution)]
    [Authorize(DatabaseCheckerPermissions.Runs.Create)]
    public async Task<Result<ComparisonRunDetailDto>> Execute([FromBody] ExecuteComparisonRunDto input)
    {
        var result = await AppService.ExecuteAsync(input);
        return result;
    }
    /// <summary>
    /// Run'in yapisal raporunu (ozet + tur/tablo gruplama + detay findings) getirir.
    /// </summary>
    /// <param name="id">Raporu istenen run'in kimligi.</param>
    /// <returns>Run'in ozet ve gruplu rapor cevap modeli.</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.RunReport)]
    public async Task<Result<ComparisonReportDto>> GetReport(Guid id)
    {
        var result = await AppService.GetReportAsync(id);
        return result;
    }
    /// <summary>
    /// Run bulgularini siddet, tur ve adres filtreleriyle kucuk sayfalar halinde getirir.
    /// </summary>
    /// <param name="id">Bulgulari okunacak run kimligi.</param>
    /// <param name="input">Filtreler ve ABP sayfalama parametreleri.</param>
    /// <param name="cancellationToken">HTTP istegi iptal edildiginde owned JSON sorgularini durduran token.</param>
    /// <returns>Filtreli toplam sayi ve cikti butcesine sigan atomik bulgular.</returns>
    [HttpGet(DatabaseCheckerHttpApiConstants.Segments.RunFindings)]
    public async Task<Result<PagedResultDto<FindingDto>>> GetFindings(
        Guid id,
        [FromQuery] FindingQueryInput input,
        CancellationToken cancellationToken)
    {
        var result = await AppService.GetFindingsAsync(id, input, cancellationToken);
        return result;
    }
}
