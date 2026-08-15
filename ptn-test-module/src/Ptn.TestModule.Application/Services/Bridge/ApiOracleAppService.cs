using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: API checker public servisini Bridge use-case'lerine baglar.
// sistemdeki gorevi: Mapperly ve Manager cagrilarini siralayan ince Application orkestrasyonudur.
[RemoteService(IsEnabled = false)]
public class ApiOracleAppService : TestModuleAppService, IApiOracleAppService
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
        var query = Mapper.Map(input);
        return Mapper.Map(_manager.Normalize(Mapper.Map(
            await _appService.SuggestOperationBindingsAsync(
                Mapper.Map(_manager.CreateOperationRequest(query, cancellationToken))))));
    }
    // Public operasyon sorgusundan uretilen request ornegini DTO'ya map eder.
    public async Task<RequestExampleDto> BuildRequestExampleAsync(
        OperationQueryDto input,
        CancellationToken cancellationToken)
    {
        await _operationQueryValidator.ValidateAndThrowAsync(input, cancellationToken);
        var query = Mapper.Map(input);
        return Mapper.Map(_manager.Normalize(Mapper.Map(
            await _appService.BuildRequestExampleAsync(
                Mapper.Map(_manager.CreateOperationRequest(query, cancellationToken))))));
    }
    // Public assertion turetilebilirlik istegini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<DerivabilityResultDto> ValidateScenarioAssertionsAsync(
        DerivabilityRequestDto input,
        CancellationToken cancellationToken)
    {
        await _derivabilityValidator.ValidateAndThrowAsync(input, cancellationToken);
        var request = Mapper.Map(input);
        return Mapper.Map(_manager.Normalize(Mapper.Map(
            await _appService.ValidateScenarioAssertionsAsync(
                Mapper.Map(_manager.PrepareDerivabilityRequest(request, cancellationToken))))));
    }
    // Public response gozlemini Domain modeline ve uygunluk sonucunu DTO'ya map eder.
    public async Task<ConformanceResultDto> AssertResponseAsync(
        ResponseObservationDto input,
        CancellationToken cancellationToken)
    {
        await _responseValidator.ValidateAndThrowAsync(input, cancellationToken);
        var observation = Mapper.Map(input);
        return Mapper.Map(_manager.Normalize(
            observation,
            Mapper.Map(await _appService.AssertResponseAsync(
                Mapper.Map(_manager.CreateResponseRequest(observation, cancellationToken))))));
    }
}
