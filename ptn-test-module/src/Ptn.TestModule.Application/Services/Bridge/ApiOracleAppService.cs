using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentValidation;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Services.Snapshots;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Api;
using CheckerListSnapshotOperationsInput = Ptn.ApiContractChecker.Dtos.Snapshots.ListSnapshotOperationsInput;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: API checker public servisini Bridge use-case'lerine baglar.
// sistemdeki gorevi: Mapperly ve Manager cagrilarini siralayan ince Application orkestrasyonudur.
[RemoteService(IsEnabled = false)]
public class ApiOracleAppService : TestModuleAppService, IApiOracleAppService
{
    private static readonly ApiOracleMapper Mapper = new();
    private readonly IResponseConformanceAppService _appService;
    private readonly ISpecSnapshotAppService _snapshotAppService;
    private readonly ApiOracleManager _manager;
    private readonly IValidator<OperationQueryDto> _operationQueryValidator;
    private readonly IValidator<OperationLinkRequestDto> _operationLinkValidator;
    private readonly IValidator<DerivabilityRequestDto> _derivabilityValidator;
    private readonly IValidator<ResponseObservationDto> _responseValidator;
    // API checker public servisini anti-corruption sinirina baglar.
    public ApiOracleAppService(
        IResponseConformanceAppService appService,
        ISpecSnapshotAppService snapshotAppService,
        ApiOracleManager manager,
        IValidator<OperationQueryDto> operationQueryValidator,
        IValidator<OperationLinkRequestDto> operationLinkValidator,
        IValidator<DerivabilityRequestDto> derivabilityValidator,
        IValidator<ResponseObservationDto> responseValidator)
    {
        _appService = appService;
        _snapshotAppService = snapshotAppService;
        _manager = manager;
        _operationQueryValidator = operationQueryValidator;
        _operationLinkValidator = operationLinkValidator;
        _derivabilityValidator = derivabilityValidator;
        _responseValidator = responseValidator;
    }
    // Checker envanterinin tum butceli sayfalarini sirayla tuketir ve tek native sonuc dondurur.
    public async Task<SnapshotOperationInventoryDto> ListSnapshotOperationsAsync(
        Guid snapshotId,
        CancellationToken cancellationToken)
    {
        var pages = new List<SnapshotOperationPage>();
        var skipCount = 0;
        long totalCount;
        do
        {
            await Task.CompletedTask.WaitAsync(cancellationToken);
            var page = Mapper.Map(await _snapshotAppService.ListOperationsAsync(
                snapshotId,
                new CheckerListSnapshotOperationsInput
                {
                    SkipCount = skipCount,
                    MaxResultCount = SnapshotOperationInventoryConsts.MaxPageSize
                }));
            pages.Add(page);
            skipCount += page.Items.Count;
            totalCount = page.TotalCount;
        }
        while (skipCount < totalCount && pages[^1].Items.Count > 0);

        return Mapper.Map(_manager.MergeOperationInventory(snapshotId, pages));
    }
    // Secili operasyonun OpenAPI links tabanli sonraki adim adaylarini normalize eder.
    public async Task<OperationLinkResultDto> SuggestOperationLinksAsync(
        OperationLinkRequestDto input,
        CancellationToken cancellationToken)
    {
        await _operationLinkValidator.ValidateAndThrowAsync(input, cancellationToken);
        var request = Mapper.Map(input);
        return Mapper.Map(_manager.Normalize(Mapper.Map(
            await _appService.SuggestOperationLinksAsync(
                Mapper.Map(_manager.PrepareOperationLinkRequest(request, cancellationToken))))));
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
