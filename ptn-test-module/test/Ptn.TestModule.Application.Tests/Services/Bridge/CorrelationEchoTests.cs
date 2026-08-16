using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Services.Conformance;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.DatabaseChecker.Services.Projections;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.FluentValidation.Bridge.Api;
using Ptn.TestModule.FluentValidation.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Services.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Checker request correlation alanlarinin sonucta aynen echo edildigini dogrular.
// sistemdeki gorevi: HAR ve batch eslestirmesinin liste konumuna geri donmesini engeller.
public class CorrelationEchoTests
{
    private const string TraceId = "0123456789abcdef0123456789abcdef";

    // DB assertion request'indeki trace ve step anahtarini sonuc DTO'sunda aynen korur.
    [Fact]
    public async Task Should_echo_database_assertion_correlation()
    {
        var checker = Substitute.For<IDatabaseAssertionAppService>();
        checker.AssertRowAsync(Arg.Any<RowAssertionRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var request = call.Arg<RowAssertionRequestDto>();
                return new RowAssertionResultDto
                {
                    OutcomeCode = AssertionOutcomeCodes.Passed,
                    Passed = true,
                    Correlation = request.Correlation
                };
            });
        var service = CreateDatabaseService(checker);
        var correlation = CreateCorrelation("db-step");

        var result = await service.AssertRowAsync(
            CreateAssertion(correlation), CancellationToken.None);

        result.Correlation!.TraceId.ShouldBe(TraceId);
        result.Correlation.StepKey.ShouldBe("db-step");
    }

    // Batch sonuc sayisi istek sayisindan farkliysa tum istekleri Unavailable yapar.
    [Fact]
    public async Task Should_make_entire_batch_unavailable_when_result_count_differs()
    {
        var checker = Substitute.For<IDatabaseAssertionAppService>();
        checker.AssertBatchAsync(
                Arg.Any<System.Collections.Generic.List<RowAssertionRequestDto>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);
        var service = CreateDatabaseService(checker);

        var result = await service.AssertBatchAsync(
            new DatabaseAssertionBatchRequestDto
            {
                Requests =
                [
                    CreateAssertion(CreateCorrelation("step-1")),
                    CreateAssertion(CreateCorrelation("step-2"))
                ]
            },
            CancellationToken.None);

        result.Count.ShouldBe(2);
        result.All(item => item.OutcomeCode == PtnOutcomeCodes.Unavailable).ShouldBeTrue();
        result.Select(item => item.Correlation!.StepKey).ShouldBe(["step-1", "step-2"]);
    }

    // API response conformance request'indeki correlation sonuc DTO'sunda aynen korunur.
    [Fact]
    public async Task Should_echo_api_conformance_correlation()
    {
        var checker = Substitute.For<IResponseConformanceAppService>();
        checker.AssertResponseAsync(Arg.Any<ResponseConformanceDto>())
            .Returns(call =>
            {
                var request = call.Arg<ResponseConformanceDto>();
                return new Ptn.ApiContractChecker.Dtos.Conformance.ConformanceResultDto
                {
                    OutcomeCode = ConformanceOutcomeCodes.Passed,
                    Correlation = request.Correlation
                };
            });
        var service = new ApiOracleAppService(
            checker,
            Substitute.For<Ptn.ApiContractChecker.Services.Snapshots.ISpecSnapshotAppService>(),
            new ApiOracleManager(),
            new OperationQueryDtoValidator(),
            new OperationLinkRequestDtoValidator(),
            new DerivabilityRequestDtoValidator(),
            new ResponseObservationDtoValidator());

        var result = await service.AssertResponseAsync(
            new ResponseObservationDto
            {
                SnapshotId = Guid.NewGuid(),
                Method = "GET",
                Path = "/tickets/1",
                StatusCode = 200,
                Correlation = CreateCorrelation("api-step")
            },
            CancellationToken.None);

        result.Correlation!.TraceId.ShouldBe(TraceId);
        result.Correlation.StepKey.ShouldBe("api-step");
    }

    // Database oracle servisini checker mock'u ve gercek validator'larla kurar.
    private static DatabaseOracleAppService CreateDatabaseService(IDatabaseAssertionAppService checker) =>
        new(
            checker,
            Substitute.For<IProjectionAppService>(),
            Substitute.For<IAssertionDerivabilityAppService>(),
            new DatabaseOracleManager(),
            new DatabaseAssertionRequestDtoValidator(),
            new DatabaseAssertionBatchRequestDtoValidator(),
            new ProjectionRequestDtoValidator(),
            new DatabaseDerivabilityRequestDtoValidator());

    // Gecerli tek assertion request'i verilen korelasyonla kurar.
    private static DatabaseAssertionRequestDto CreateAssertion(CorrelationRefDto correlation) => new()
    {
        ConnectionId = Guid.NewGuid(),
        SchemaName = "public",
        TableName = "tickets",
        TimeoutMs = 1000,
        PollIntervalMs = 100,
        Cardinality = new DatabaseCardinalityExpectationDto { KindCode = "Exactly" },
        Correlation = correlation
    };

    // Testler icin gecerli W3C trace ve kapali step anahtari kurar.
    private static CorrelationRefDto CreateCorrelation(string stepKey) => new()
    {
        TraceId = TraceId,
        StepKey = stepKey
    };
}
