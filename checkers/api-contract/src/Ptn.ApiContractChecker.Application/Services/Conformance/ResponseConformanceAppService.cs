using FluentValidation;
using Ptn.ApiContractChecker.Application.Mappers.Conformance;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Services.Conformance;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Ptn.ApiContractChecker.Diagnostics;
using Ptn.ApiContractChecker.Constants.Diagnostics;

namespace Ptn.ApiContractChecker.Application.Services.Conformance;

// islevi: Snapshot okuma, DTO mapping ve conformance manager cagrilarini duz sirada orkestre eder.
// sistemdeki gorevi: HTTP yuzeyi ile domain oracle'i arasindaki ince application katmanidir.
[RemoteService(IsEnabled = false)]
public class ResponseConformanceAppService : ApiContractCheckerAppService, IResponseConformanceAppService
{
    private static readonly ConformanceMapper Mapper = new();
    private ISpecSnapshotRepository Repository => LazyGetRequiredService<ISpecSnapshotRepository>();
    private ResponseConformanceManager Manager => LazyGetRequiredService<ResponseConformanceManager>();
    private RequestExampleBuilder ExampleBuilder => LazyGetRequiredService<RequestExampleBuilder>();
    private OperationBindingSuggester BindingSuggester => LazyGetRequiredService<OperationBindingSuggester>();
    private IValidator<ResponseConformanceDto> ResponseValidator =>
        LazyGetRequiredService<IValidator<ResponseConformanceDto>>();
    private IValidator<RequestConformanceDto> RequestValidator =>
        LazyGetRequiredService<IValidator<RequestConformanceDto>>();
    private IValidator<OperationSelectionDto> SelectionValidator =>
        LazyGetRequiredService<IValidator<OperationSelectionDto>>();
    private IValidator<AssertionDerivabilityDto> AssertionValidator =>
        LazyGetRequiredService<IValidator<AssertionDerivabilityDto>>();
    private AssertionDerivabilityManager AssertionManager =>
        LazyGetRequiredService<AssertionDerivabilityManager>();
    private SampleSetManager SampleManager => LazyGetRequiredService<SampleSetManager>();
    private OperationLinkSuggester LinkSuggester => LazyGetRequiredService<OperationLinkSuggester>();
    private IValidator<SampleSetRequestDto> SampleValidator =>
        LazyGetRequiredService<IValidator<SampleSetRequestDto>>();
    private IValidator<OperationLinkRequestDto> LinkValidator =>
        LazyGetRequiredService<IValidator<OperationLinkRequestDto>>();

    public ResponseConformanceAppService(IAbpLazyServiceProvider provider) : base(provider)
    {
    }

    public async Task<ConformanceResultDto> AssertResponseAsync(ResponseConformanceDto input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.ConformanceAssertSpan,
            ApiContractCheckerDiagnostics.MomentRuntime);
        await ResponseValidator.ValidateAndThrowAsync(input);
        var snapshot = await Repository.FindWithDetailsAsync(input.SnapshotId);
        var request = Mapper.MapToRequest(input);
        var result = await Manager.AssertResponseAsync(snapshot, request);
        var response = Mapper.MapToDto(result);
        ApiContractCheckerActivity.SetResponseBytes(activity, result.MeasureUtf8Bytes());
        return response;
    }

    public async Task<ConformanceResultDto> AssertRequestAsync(RequestConformanceDto input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.ConformanceAssertSpan,
            ApiContractCheckerDiagnostics.MomentAuthoring);
        await RequestValidator.ValidateAndThrowAsync(input);
        var snapshot = await Repository.FindWithDetailsAsync(input.SnapshotId);
        var request = Mapper.MapToRequest(input);
        var result = await Manager.AssertRequestAsync(snapshot, request);
        var response = Mapper.MapToDto(result);
        ApiContractCheckerActivity.SetResponseBytes(activity, result.MeasureUtf8Bytes());
        return response;
    }

    public async Task<RequestExampleDto> BuildRequestExampleAsync(OperationSelectionDto input)
    {
        await SelectionValidator.ValidateAndThrowAsync(input);
        var snapshot = await Repository.FindWithDetailsAsync(input.SnapshotId);
        var request = Mapper.MapToSelection(input);
        var result = await ExampleBuilder.BuildAsync(snapshot, request);
        return Mapper.MapToDto(result);
    }

    public async Task<OperationBindingResultDto> SuggestOperationBindingsAsync(OperationSelectionDto input)
    {
        await SelectionValidator.ValidateAndThrowAsync(input);
        var snapshot = await Repository.FindWithDetailsAsync(input.SnapshotId);
        var request = Mapper.MapToSelection(input);
        var result = await BindingSuggester.SuggestAsync(snapshot, request);
        return Mapper.MapToDto(result);
    }

    // islevi: Assertion yollarini yalniz snapshot response semasindan turetilebilirlik kontrolune verir.
    public async Task<AssertionDerivabilityResultDto> ValidateScenarioAssertionsAsync(
        AssertionDerivabilityDto input)
    {
        using var activity = ApiContractCheckerActivity.Start(
            ApiContractCheckerDiagnostics.ConformanceAssertSpan,
            ApiContractCheckerDiagnostics.MomentAuthoring);
        await AssertionValidator.ValidateAndThrowAsync(input);
        var snapshot = await Repository.FindWithDetailsAsync(input.SnapshotId);
        var request = Mapper.MapToRequest(input);
        var result = await AssertionManager.ValidateAsync(snapshot, request);
        var response = Mapper.MapToDto(result);
        ApiContractCheckerActivity.SetResponseBytes(activity, result.MeasureUtf8Bytes());
        return response;
    }

    // islevi: Public sample istegini dogrulayip snapshot ve domain ureticileriyle orkestre eder.
    public async Task<SampleSetResultDto> BuildSampleSetAsync(SampleSetRequestDto input)
    {
        await SampleValidator.ValidateAndThrowAsync(input);
        var snapshot = await Repository.FindWithDetailsAsync(input.SnapshotId);
        var request = Mapper.MapToRequest(input);
        var result = await SampleManager.BuildAsync(snapshot, request);
        return Mapper.MapToDto(result);
    }

    // islevi: Public kaynak operasyon istegini dogrulayip esik ustu link adaylarina cevirir.
    public async Task<OperationLinkResultDto> SuggestOperationLinksAsync(OperationLinkRequestDto input)
    {
        await LinkValidator.ValidateAndThrowAsync(input);
        var snapshot = await Repository.FindWithDetailsAsync(input.SnapshotId);
        var request = Mapper.MapToRequest(input);
        var result = await LinkSuggester.SuggestAsync(snapshot, request);
        return Mapper.MapToDto(result);
    }
}
