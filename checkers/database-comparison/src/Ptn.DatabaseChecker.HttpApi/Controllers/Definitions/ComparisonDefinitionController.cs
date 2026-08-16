using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Definitions;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Definitions;
using System;
using System.Threading.Tasks;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Definitions;

// islevi: Karsilastirma tanimi okuma/yazma endpointlerini HTTP uzerinden acar.
// sistemdeki gorevi: ComparisonDefinition detay, liste, create ve update isteklerini IComparisonDefinitionAppService'e yonlendiren ince controller katmanidir.
/// <summary>
/// Karsilastirma tanimi islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.ComparisonDefinitions)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Definitions)]
[Authorize(DatabaseCheckerPermissions.Definitions.View)]
public class ComparisonDefinitionController : EntityReadControllerBase<IComparisonDefinitionAppService, ComparisonDefinitionDto>
{
    /// <summary>
    /// Yeni karsilastirma tarifi olusturur.
    /// </summary>
    /// <param name="input">Tarif olusturma istegi.</param>
    /// <returns>Olusturulan tarifin cevap modeli.</returns>
    [HttpPost]
    [Authorize(DatabaseCheckerPermissions.Definitions.Manage)]
    public async Task<Result<ComparisonDefinitionDto>> Create([FromBody] CreateComparisonDefinitionDto input)
    {
        var result = await AppService.CreateAsync(input);
        return result;
    }
    /// <summary>
    /// Mevcut karsilastirma tarifini gunceller.
    /// </summary>
    /// <param name="id">Guncellenecek tarifin kimligi.</param>
    /// <param name="input">Tarif guncelleme istegi.</param>
    /// <returns>Guncellenen tarifin cevap modeli.</returns>
    [HttpPut(DatabaseCheckerHttpApiConstants.Segments.EntityById)]
    [Authorize(DatabaseCheckerPermissions.Definitions.Manage)]
    public async Task<Result<ComparisonDefinitionDto>> Update(Guid id, [FromBody] UpdateComparisonDefinitionDto input)
    {
        var result = await AppService.UpdateAsync(id, input);
        return result;
    }

}
