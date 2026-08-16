using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Conformance;
using SystemStandards.Results;

namespace Ptn.ApiContractChecker.Controllers.Conformance;

// islevi: Response/request oracle'i ile request yazarlik yardimcilarini HTTP uzerinden acar.
// sistemdeki gorevi: Test Module cagrilarini yetkilendirip ince AppService transport sarmalayicisina yonlendirir.
/// <summary>API request ve response uygunluk islemleri.</summary>
[Route(ApiContractCheckerRoutes.Conformance)]
[ApiExplorerSettings(GroupName = ApiContractCheckerSwaggerConstants.ConformanceGroupName)]
[Authorize(ApiContractCheckerPermissions.Conformance.Execute)]
public class ResponseConformanceController : ApiContractCheckerController
{
    private IResponseConformanceAppService AppService =>
        LazyGetRequiredService<IResponseConformanceAppService>();

    /// <summary>Gozlenen response'un snapshot sozlesmesine uygunlugunu denetler.</summary>
    /// <param name="input">Snapshot, operasyon ve gozlenen response.</param>
    /// <returns>Kapali outcome kodu ile deger icermeyen ihlaller.</returns>
    [HttpPost(ApiContractCheckerRoutes.ConformanceResponse)]
    public async Task<Result<ConformanceResultDto>> AssertResponse(ResponseConformanceDto input)
    {
        var result = await AppService.AssertResponseAsync(input);
        return result;
    }

    /// <summary>Gonderilecek request'in snapshot sozlesmesine uygunlugunu denetler.</summary>
    /// <param name="input">Snapshot, operasyon ve gonderilecek request.</param>
    /// <returns>Kapali outcome kodu ile deger icermeyen ihlaller.</returns>
    [HttpPost(ApiContractCheckerRoutes.ConformanceRequest)]
    public async Task<Result<ConformanceResultDto>> AssertRequest(RequestConformanceDto input)
    {
        var result = await AppService.AssertRequestAsync(input);
        return result;
    }

    /// <summary>Operasyon icin minimal placeholder request iskeleti uretir.</summary>
    /// <param name="input">Snapshot ve hedef operasyon secimi.</param>
    /// <returns>En fazla 2 KB request iskeleti.</returns>
    [HttpPost(ApiContractCheckerRoutes.ConformanceRequestExample)]
    public async Task<Result<RequestExampleDto>> BuildRequestExample(OperationSelectionDto input)
    {
        var result = await AppService.BuildRequestExampleAsync(input);
        return result;
    }

    /// <summary>Hedef operasyon icin isim ve tip uyumlu onceki operasyonlari onerir.</summary>
    /// <param name="input">Snapshot ve hedef operasyon secimi.</param>
    /// <returns>En fazla bes aciklanabilir alan baglama onerisi.</returns>
    [HttpPost(ApiContractCheckerRoutes.ConformanceOperationBindings)]
    public async Task<Result<OperationBindingResultDto>> SuggestOperationBindings(OperationSelectionDto input)
    {
        var result = await AppService.SuggestOperationBindingsAsync(input);
        return result;
    }

    /// <summary>Senaryo assertion yollarinin sozlesmeden turetilebilirligini denetler.</summary>
    [HttpPost(ApiContractCheckerRoutes.ConformanceAssertionDerivability)]
    public async Task<Result<AssertionDerivabilityResultDto>> ValidateScenarioAssertions(
        AssertionDerivabilityDto input)
    {
        var result = await AppService.ValidateScenarioAssertionsAsync(input);
        return result;
    }

    /// <summary>Operasyon request semasindan sinir ve negatif alan ornekleri uretir.</summary>
    [HttpPost(ApiContractCheckerRoutes.ConformanceSampleSets)]
    [Authorize(ApiContractCheckerPermissions.Conformance.GenerateSamples)]
    public async Task<Result<SampleSetResultDto>> BuildSampleSet(SampleSetRequestDto input)
    {
        var result = await AppService.BuildSampleSetAsync(input);
        return result;
    }

    /// <summary>Kaynak operasyon icin kanitli ve insan onayli sonraki operasyon adaylari onerir.</summary>
    [HttpPost(ApiContractCheckerRoutes.ConformanceOperationLinks)]
    [Authorize(ApiContractCheckerPermissions.Conformance.SuggestLinks)]
    public async Task<Result<OperationLinkResultDto>> SuggestOperationLinks(OperationLinkRequestDto input)
    {
        var result = await AppService.SuggestOperationLinksAsync(input);
        return result;
    }
}
