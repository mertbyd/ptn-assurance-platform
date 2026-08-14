using System.Threading;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: API checker public servisini Bridge portuna baglar.
// sistemdeki gorevi: Mapperly ve Manager cagrilarini siralayan ince Application orkestrasyonudur.
[RemoteService(IsEnabled = false)]
public class ApiOracleAppService : TestModuleAppService, IApiOraclePort
{
    private static readonly ApiOracleMapper Mapper = new();
    private readonly IResponseConformanceAppService _appService;
    private readonly ApiOracleManager _manager;

    // API checker public servisini anti-corruption sinirina baglar.
    public ApiOracleAppService(IResponseConformanceAppService appService, ApiOracleManager manager)
    {
        _appService = appService;
        _manager = manager;
    }

    // Operasyon sorgusunu checker'a iletip normalize Bridge sonucunu dondurur.
    public async Task<PtnOperationBinding> SuggestOperationBindingsAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.SuggestOperationBindingsAsync(Mapper.Map(query))));
    }

    // Request ornegi yazarligini checker'a iletip Bridge modeline cevirir.
    public async Task<PtnRequestExample> BuildRequestExampleAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.BuildRequestExampleAsync(Mapper.Map(query))));
    }

    // Assertion turetilebilirligini checker'a iletip kapali outcome modelini dondurur.
    public async Task<PtnDerivabilityResult> ValidateScenarioAssertionsAsync(
        PtnDerivabilityRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.ValidateScenarioAssertionsAsync(Mapper.Map(request))));
    }

    // Gozlenen response'u checker'a iletip normalize uygunluk sonucunu dondurur.
    public async Task<PtnConformanceResult> AssertResponseAsync(
        PtnResponseObservation observation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertResponseAsync(Mapper.Map(observation))));
    }
}
