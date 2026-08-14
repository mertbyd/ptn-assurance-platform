using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Services.Bridge;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.FluentValidation.Bridge.Api;
using Ptn.TestModule.Managers.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: API oracle adapter'inin outcome casing'i ve operasyon baglama DTO cevirisini dogrular.
// sistemdeki gorevi: API checker ham sozlugunun domain portundan sizmasini test kapisiyla engeller.
public class ApiOracleAppServiceTests
{
    // Lowercase checker outcome'unu PascalCase kopru outcome'una cevirir.
    [Fact]
    public async Task Should_normalize_api_outcome_and_map_suggestions()
    {
        var service = Substitute.For<IResponseConformanceAppService>();
        service.SuggestOperationBindingsAsync(Arg.Any<OperationSelectionDto>()).Returns(
            new OperationBindingResultDto
            {
                OutcomeCode = ConformanceOutcomeCodes.Passed,
                Suggestions =
                [
                    new OperationBindingSuggestionDto
                    {
                        SourceOperationId = "source-operation",
                        SourceMethod = "POST",
                        SourcePath = "/tickets",
                        Score = 95
                    }
                ]
            });
        var oracleService = new ApiOracleAppService(
            service,
            new ApiOracleManager(),
            new OperationQueryDtoValidator(),
            new DerivabilityRequestDtoValidator(),
            new ResponseObservationDtoValidator());

        var result = await oracleService.SuggestOperationBindingsAsync(
            new OperationQueryDto
            {
                SnapshotId = System.Guid.NewGuid(),
                Method = "GET",
                Path = "/tickets/{id}"
            },
            CancellationToken.None);

        result.OutcomeCode.ShouldBe(PtnOutcomeCodes.Passed);
        result.Suggestions.Single().Score.ShouldBe(95);
    }
}
