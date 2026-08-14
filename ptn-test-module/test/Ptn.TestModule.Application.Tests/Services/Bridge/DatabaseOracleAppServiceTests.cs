using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Services.Bridge;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.FluentValidation.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Shouldly;
using Xunit;
using CheckerFailedExpectationDto = Ptn.DatabaseChecker.Dtos.Assertions.FailedExpectationDto;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Database oracle adapter'inin outcome casing'i, redaksiyon ve projeksiyon dususunu dogrular.
// sistemdeki gorevi: Ham satir degerlerinin sizmasini ve olmayan projeksiyon yuzeyinin taklit edilmesini engeller.
public class DatabaseOracleAppServiceTests
{
    // Checker satir sonucunu normalize eder ve tum deger alanlarini redakte eder.
    [Fact]
    public async Task Should_normalize_and_redact_database_assertion_result()
    {
        var service = Substitute.For<IDatabaseAssertionAppService>();
        service.AssertRowAsync(Arg.Any<RowAssertionRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new RowAssertionResultDto
            {
                OutcomeCode = AssertionOutcomeCodes.ValueMismatch,
                RowSummary = new Dictionary<string, string?> { ["email"] = "secret@example.test" },
                FailedExpectations =
                [
                    new CheckerFailedExpectationDto
                    {
                        ColumnName = "email",
                        MatcherKindCode = "Equals",
                        ExpectedValue = "expected@example.test",
                        ObservedValue = "secret@example.test"
                    }
                ]
            });
        var oracleService = CreateService(service);

        var result = await oracleService.AssertRowAsync(
            new DatabaseAssertionRequestDto
            {
                ConnectionId = System.Guid.NewGuid(),
                SchemaName = "identity",
                TableName = "users",
                TimeoutMs = 1000,
                PollIntervalMs = 100,
                Cardinality = new DatabaseCardinalityExpectationDto { KindCode = "Exactly" }
            },
            CancellationToken.None);

        result.OutcomeCode.ShouldBe(PtnOutcomeCodes.ValueMismatch);
        result.RowSummary!["email"].ShouldBe(PtnRedactionCodes.Redacted);
        result.FailedExpectations[0].ObservedValue.ShouldBe(PtnRedactionCodes.Redacted);
    }

    // Checker projeksiyon yuzeyi bulunmadiginda assertion hilesi yapmadan Unavailable dondurur.
    [Fact]
    public async Task Should_return_unavailable_when_projection_surface_is_missing()
    {
        var oracleService = CreateService(Substitute.For<IDatabaseAssertionAppService>());

        var result = await oracleService.ProjectAsync(
            new ProjectionRequestDto
            {
                ConnectionId = System.Guid.NewGuid(),
                DbSchemaName = "identity",
                TableName = "users",
                MaxRows = 1
            },
            CancellationToken.None);

        result.StateCode.ShouldBe(PtnEvidenceStateCodes.Unavailable);
    }

    // Servisi gercek Application.Contracts validator'lariyla kurar.
    private static DatabaseOracleAppService CreateService(IDatabaseAssertionAppService appService) =>
        new(
            appService,
            new DatabaseOracleManager(),
            new DatabaseAssertionRequestDtoValidator(),
            new DatabaseAssertionBatchRequestDtoValidator(),
            new ProjectionRequestDtoValidator());
}
