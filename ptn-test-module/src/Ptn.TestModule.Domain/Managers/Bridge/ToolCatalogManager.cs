using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Aktif tool butcesini ve talep uzerine kesfedilen toolset yuzeyini kurar.
// sistemdeki gorevi: Ajan baglamina en fazla yedi tool tasir ve kademe-4 eylemlerini katalog disinda tutar.
public class ToolCatalogManager : TestModuleDomainService
{
    // Aktif ve discoverable tool kodlarini kararli sirada kataloglar.
    public ToolCatalog GetCatalog(string responseFormat)
    {
        EnsureBudget(PtnToolCodes.Active);
        return new ToolCatalog
        {
            ResponseFormat = responseFormat,
            ActiveToolCodes = PtnToolCodes.Active.ToList(),
            DiscoverableToolCodes = PtnToolCodes.All.Except(PtnToolCodes.Active).ToList(),
            ToolsetCodes = [PtnToolCodes.AuthoringToolset, PtnToolCodes.RuntimeToolset],
            ResourceLink = ResourceLink(responseFormat)
        };
    }
    // Secilen toolset icin yalniz o grubun kapali tool kodlarini dondurur.
    public IReadOnlyList<string> Discover(string toolsetCode) => toolsetCode switch
    {
        PtnToolCodes.AuthoringToolset => [PtnToolCodes.Ground, PtnToolCodes.Validate, PtnToolCodes.Knowledge],
        PtnToolCodes.RuntimeToolset => [PtnToolCodes.Run, PtnToolCodes.Result, PtnToolCodes.Explain, PtnToolCodes.Impact],
        _ => []
    };
    // Aktif tool sayisinin urun butcesini asmadigini mekanik olarak korur.
    private static void EnsureBudget(IReadOnlyCollection<string> activeTools)
    {
        if (activeTools.Count > PtnToolCodes.ActiveMax)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ToolBudgetExceeded);
        }
    }
    // Concise cevapta agir katalog semasini dinamik resource adresine tasir.
    private static string? ResourceLink(string responseFormat) =>
        responseFormat == PtnResponseFormatCodes.Concise
            ? PtnBridgeRoutes.Resource(PtnToolCodes.Knowledge)
            : null;
}
