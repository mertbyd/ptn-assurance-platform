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
            Substitute.For<Ptn.ApiContractChecker.Services.Snapshots.ISpecSnapshotAppService>(),
            new ApiOracleManager(),
            new OperationQueryDtoValidator(),
            new OperationLinkRequestDtoValidator(),
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

    // OpenAPI links adaylarini kaynak sozlugu ve parametre eslemeleriyle Bridge'e tasir.
    [Fact]
    public async Task Should_expose_operation_link_candidates_without_guessing()
    {
        var service = Substitute.For<IResponseConformanceAppService>();
        service.SuggestOperationLinksAsync(
            Arg.Any<Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkRequestDto>()).Returns(
            new Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkResultDto
            {
                OutcomeCode = ConformanceOutcomeCodes.Passed,
                Candidates =
                [
                    new Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkCandidateDto
                    {
                        TargetOperationId = "get-order",
                        SourceCode = OperationLinkSourceCodes.DeclaredLink,
                        Score = 100,
                        RequiresHumanApproval = true,
                        ParameterMap =
                        [
                            new Ptn.ApiContractChecker.Dtos.Conformance.OperationLinkParameterBindingDto
                            {
                                SourceResponsePointer = "/body/orderId",
                                TargetParameterName = "orderId"
                            }
                        ]
                    }
                ]
            });
        var oracleService = new ApiOracleAppService(
            service,
            Substitute.For<Ptn.ApiContractChecker.Services.Snapshots.ISpecSnapshotAppService>(),
            new ApiOracleManager(),
            new OperationQueryDtoValidator(),
            new OperationLinkRequestDtoValidator(),
            new DerivabilityRequestDtoValidator(),
            new ResponseObservationDtoValidator());

        var result = await oracleService.SuggestOperationLinksAsync(
            new Ptn.TestModule.Dtos.Bridge.Api.OperationLinkRequestDto
            {
                SnapshotId = System.Guid.NewGuid(),
                SourceOperationId = "create-order"
            },
            CancellationToken.None);

        result.OutcomeCode.ShouldBe(PtnOutcomeCodes.Passed);
        result.Candidates.Single().SourceCode.ShouldBe(PtnOperationLinkSourceCodes.DeclaredLink);
        result.Candidates.Single().ParameterMap.Single().TargetParameterName.ShouldBe("orderId");
        result.Candidates.Single().RequiresHumanApproval.ShouldBeTrue();
    }

    // Checker sayfalarini toplam sayiya kadar tuketip her satira kararli kapali referans verir.
    [Fact]
    public async Task Should_consume_the_complete_snapshot_operation_inventory()
    {
        var conformance = Substitute.For<IResponseConformanceAppService>();
        var snapshots = Substitute.For<Ptn.ApiContractChecker.Services.Snapshots.ISpecSnapshotAppService>();
        snapshots.ListOperationsAsync(
                Arg.Any<System.Guid>(),
                Arg.Any<Ptn.ApiContractChecker.Dtos.Snapshots.ListSnapshotOperationsInput>())
            .Returns(call =>
            {
                var input = call.Arg<Ptn.ApiContractChecker.Dtos.Snapshots.ListSnapshotOperationsInput>();
                return input.SkipCount == 0
                    ? new Ptn.ApiContractChecker.Dtos.Snapshots.SnapshotOperationInventoryDto
                    {
                        OutcomeCode = ConformanceOutcomeCodes.Passed,
                        TotalCount = 2,
                        Items =
                        [
                            new Ptn.ApiContractChecker.Dtos.Snapshots.SnapshotOperationRowDto
                            {
                                OperationId = "create-ticket",
                                Method = "POST",
                                Path = "/tickets"
                            }
                        ]
                    }
                    : new Ptn.ApiContractChecker.Dtos.Snapshots.SnapshotOperationInventoryDto
                    {
                        OutcomeCode = ConformanceOutcomeCodes.Passed,
                        TotalCount = 2,
                        Items =
                        [
                            new Ptn.ApiContractChecker.Dtos.Snapshots.SnapshotOperationRowDto
                            {
                                OperationId = "get-ticket",
                                Method = "GET",
                                Path = "/tickets/{id}"
                            }
                        ]
                    };
            });
        var oracleService = new ApiOracleAppService(
            conformance,
            snapshots,
            new ApiOracleManager(),
            new OperationQueryDtoValidator(),
            new OperationLinkRequestDtoValidator(),
            new DerivabilityRequestDtoValidator(),
            new ResponseObservationDtoValidator());

        var result = await oracleService.ListSnapshotOperationsAsync(
            System.Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CancellationToken.None);

        result.IsComplete.ShouldBeTrue();
        result.SnapshotId.ShouldBe(System.Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result.Items.Count.ShouldBe(2);
        result.Items.Select(item => item.ReferenceId).Distinct().Count().ShouldBe(2);
        await snapshots.Received(2).ListOperationsAsync(
            Arg.Any<System.Guid>(),
            Arg.Any<Ptn.ApiContractChecker.Dtos.Snapshots.ListSnapshotOperationsInput>());
    }
}
