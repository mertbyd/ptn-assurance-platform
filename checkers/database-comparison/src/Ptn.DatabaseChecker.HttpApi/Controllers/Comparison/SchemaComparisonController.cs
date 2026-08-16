using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Dtos.Comparison;
using Ptn.DatabaseChecker.Dtos.Findings;
using Ptn.DatabaseChecker.Permissions;
using Ptn.DatabaseChecker.Services.Comparison;
using SystemStandards.Results;

namespace Ptn.DatabaseChecker.Controllers.Comparison;

// islevi: Iki baglanti arasinda sema karsilastirma endpoint'ini HTTP uzerinden acar.
// sistemdeki gorevi: Istegi ISchemaComparisonAppService'e yonlendiren ince controller; canli iki hedefi okuyup yon bilgili yapisal farklari doner, sifre alanlari cevaba girmez.
/// <summary>
/// Iki veritabani arasinda sema karsilastirma islemleri.
/// </summary>
[Route(DatabaseCheckerHttpApiConstants.Routes.SchemaComparison)]
[ApiExplorerSettings(GroupName = DatabaseCheckerHttpApiConstants.Groups.Comparison)]
[Authorize(DatabaseCheckerPermissions.Connections.Manage)]
public class SchemaComparisonController : DatabaseCheckerController
{
    private ISchemaComparisonAppService AppService => LazyServiceProvider.LazyGetRequiredService<ISchemaComparisonAppService>();
    /// <summary>
    /// Kaynak ve hedef baglantinin semalarini kapsam filtresiyle (Include/Exclude/Ignore) kiyaslar; tablo/kolon/index/constraint/view/trigger/routine yon bilgili yapisal farklarini doner.
    /// </summary>
    /// <param name="input">Kaynak/hedef baglanti kimlikleri, opsiyonel sema filtresi ve kapsam kurallari.</param>
    /// <returns>Yapisal karsilastirma bulgulari (sifre icermez).</returns>
    [HttpPost]
    public async Task<Result<ComparisonFindingsDto>> Compare([FromBody] CompareSchemaRequestDto input)
    {
        var result = await AppService.CompareSchemaAsync(input);
        return result;
    }
}
