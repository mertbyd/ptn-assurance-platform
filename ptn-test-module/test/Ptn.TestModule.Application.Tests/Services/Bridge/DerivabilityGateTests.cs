using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.DatabaseChecker.Services.Projections;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.FluentValidation.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Services.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: DB assertion turetilebilirlik sonucunun Bridge yayin kapisina tasindigini dogrular.
// sistemdeki gorevi: x-checknexus-db assertion'larinin API kapisindan bagimsiz sessizce gecmesini engeller.
public class DerivabilityGateTests
{
    // Tek turetilemeyen DB assertion toplu kapinin AllDerivable sonucunu dusurur.
    [Fact]
    public async Task Should_lower_gate_for_non_derivable_database_assertion()
    {
        var derivability = Substitute.For<IAssertionDerivabilityAppService>();
        derivability.ValidateDerivabilityAsync(
                Arg.Any<DerivabilityRequestDto>(),
                Arg.Any<CancellationToken>())
            .Returns(new DerivabilityResultDto
            {
                Assertions =
                [
                    new DerivabilityItemDto
                    {
                        TableRef = "public.tickets",
                        ColumnRef = "missing_column",
                        OutcomeCode = AssertionDerivabilityCodes.ColumnNotFound
                    }
                ]
            });
        var service = CreateService(derivability);

        var result = await service.ValidateDerivabilityAsync(
            new DatabaseDerivabilityRequestDto
            {
                ConnectionId = Guid.NewGuid(),
                Assertions =
                [
                    new DatabaseDerivabilityAddressDto
                    {
                        SchemaName = "public",
                        TableName = "tickets",
                        KeyColumns = ["id"],
                        ExpectedColumns = ["missing_column"],
                        MatcherCode = "Equals",
                        CardinalityKindCode = "Exactly"
                    }
                ]
            },
            CancellationToken.None);

        result.AllDerivable.ShouldBeFalse();
        result.Assertions[0].OutcomeCode.ShouldBe(Ptn.TestModule.Constants.Bridge.Vocabulary.PtnOutcomeCodes.ColumnNotFound);
    }

    // Servisi DB derivability yuzeyi ve repository-native validator'larla kurar.
    private static DatabaseOracleAppService CreateService(IAssertionDerivabilityAppService derivability) =>
        new(
            Substitute.For<IDatabaseAssertionAppService>(),
            Substitute.For<IProjectionAppService>(),
            derivability,
            new DatabaseOracleManager(),
            new DatabaseAssertionRequestDtoValidator(),
            new DatabaseAssertionBatchRequestDtoValidator(),
            new ProjectionRequestDtoValidator(),
            new DatabaseDerivabilityRequestDtoValidator());
}
