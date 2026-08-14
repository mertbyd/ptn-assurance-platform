using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: API checker public servisini Bridge portuna baglar.
// sistemdeki gorevi: Mapperly ve Manager cagrilarini siralayan ince Application orkestrasyonudur.
[RemoteService(IsEnabled = false)]
public class ApiOracleAppService : TestModuleAppService, IApiOracleAppService, IApiOraclePort
{
    private static readonly ApiOracleMapper Mapper = new();
    private readonly IResponseConformanceAppService _appService;
    private readonly ApiOracleManager _manager;
    private readonly IValidator<OperationQueryDto> _operationQueryValidator;
    private readonly IValidator<DerivabilityRequestDto> _derivabilityValidator;
    private readonly IValidator<ResponseObservationDto> _responseValidator;

    // API checker public servisini anti-corruption sinirina baglar.
    public ApiOracleAppService(
        IResponseConformanceAppService appService,
        ApiOracleManager manager,
        IValidator<OperationQueryDto> operationQueryValidator,
        IValidator<DerivabilityRequestDto> derivabilityValidator,
        IValidator<ResponseObservationDto> responseValidator)
    {
        _appService = appService;
        _manager = manager;
        _operationQueryValidator = operationQueryValidator;
        _derivabilityValidator = derivabilityValidator;
        _responseValidator = responseValidator;
    }

    // Public operasyon sorgusunu Domain modeline map edip normalize DTO sonucu dondurur.
    public async Task<OperationBindingDto> SuggestOperationBindingsAsync(
        OperationQueryDto input,
        CancellationToken cancellationToken)
    {
        await _operationQueryValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IApiOraclePort)this).SuggestOperationBindingsAsync(Mapper.Map(input), cancellationToken));
    }

    // Public operasyon sorgusundan uretilen request ornegini DTO'ya map eder.
    public async Task<RequestExampleDto> BuildRequestExampleAsync(
        OperationQueryDto input,
        CancellationToken cancellationToken)
    {
        await _operationQueryValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IApiOraclePort)this).BuildRequestExampleAsync(Mapper.Map(input), cancellationToken));
    }

    // Public assertion turetilebilirlik istegini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<DerivabilityResultDto> ValidateScenarioAssertionsAsync(
        DerivabilityRequestDto input,
        CancellationToken cancellationToken)
    {
        await _derivabilityValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IApiOraclePort)this).ValidateScenarioAssertionsAsync(Mapper.Map(input), cancellationToken));
    }

    // Public response gozlemini Domain modeline ve uygunluk sonucunu DTO'ya map eder.
    public async Task<ConformanceResultDto> AssertResponseAsync(
        ResponseObservationDto input,
        CancellationToken cancellationToken)
    {
        await _responseValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IApiOraclePort)this).AssertResponseAsync(Mapper.Map(input), cancellationToken));
    }

    // Operasyon sorgusunu checker'a iletip normalize Bridge sonucunu dondurur.
    async Task<PtnOperationBinding> IApiOraclePort.SuggestOperationBindingsAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.SuggestOperationBindingsAsync(
                Mapper.Map(_manager.CreateOperationRequest(query)))));
    }

    // Request ornegi yazarligini checker'a iletip Bridge modeline cevirir.
    async Task<PtnRequestExample> IApiOraclePort.BuildRequestExampleAsync(
        PtnOperationQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.BuildRequestExampleAsync(
                Mapper.Map(_manager.CreateOperationRequest(query)))));
    }

    // Assertion turetilebilirligini checker'a iletip kapali outcome modelini dondurur.
    async Task<PtnDerivabilityResult> IApiOraclePort.ValidateScenarioAssertionsAsync(
        PtnDerivabilityRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.ValidateScenarioAssertionsAsync(Mapper.Map(request))));
    }

    // Gozlenen response'u checker'a iletip normalize uygunluk sonucunu dondurur.
    async Task<PtnConformanceResult> IApiOraclePort.AssertResponseAsync(
        PtnResponseObservation observation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertResponseAsync(
                Mapper.Map(_manager.CreateResponseRequest(observation)))));
    }
}
