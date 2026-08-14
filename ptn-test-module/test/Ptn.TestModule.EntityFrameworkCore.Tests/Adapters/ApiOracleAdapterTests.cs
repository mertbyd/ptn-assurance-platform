using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Adapters;

// islevi: API oracle adapter'inin outcome casing'i ve operasyon baglama DTO cevirisini dogrular.
// sistemdeki gorevi: API checker ham sozlugunun domain portundan sizmasini test kapisiyla engeller.
public class ApiOracleAdapterTests
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
        var adapter = new ApiOracleAdapter(service);

        var result = await adapter.SuggestOperationBindingsAsync(
            new PtnOperationQuery { Method = "GET", Path = "/tickets/{id}" },
            CancellationToken.None);

        result.OutcomeCode.ShouldBe(PtnOutcomeCodes.Passed);
        result.Suggestions.Single().Score.ShouldBe(95);
    }
}
