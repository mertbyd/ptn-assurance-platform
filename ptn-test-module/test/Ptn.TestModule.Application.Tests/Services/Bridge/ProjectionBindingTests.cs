using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison.Projections;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.DatabaseChecker.Services.Projections;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.FluentValidation.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Services.Bridge;
using Shouldly;
using Xunit;
using CheckerProjectionRequestDto = Ptn.DatabaseChecker.Dtos.Projections.ProjectionRequestDto;
using CheckerProjectionResultDto = Ptn.DatabaseChecker.Dtos.Projections.ProjectionResultDto;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Salt-okunur checker projection yuzeyinin Bridge kanit durumuna baglandigini dogrular.
// sistemdeki gorevi: Okunamayan kanitin yanlis yokluk veya yetki hukmune donusmesini engeller.
public class ProjectionBindingTests
{
    // Checker NotAuthorized sonucunu yalniz kanit toplanamadi anlamindaki Unavailable'a indirger.
    [Fact]
    public async Task Should_make_unreadable_projection_inconclusive_evidence()
    {
        var projection = Substitute.For<IProjectionAppService>();
        projection.ProjectRowsAsync(Arg.Any<CheckerProjectionRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CheckerProjectionResultDto
            {
                OutcomeCode = ProjectionOutcomeCodes.NotAuthorized
            });
        var service = CreateService(projection);

        var result = await service.ProjectAsync(
            new ProjectionRequestDto
            {
                ConnectionId = Guid.NewGuid(),
                DbSchemaName = "public",
                TableName = "tickets",
                MaxRows = 1
            },
            CancellationToken.None);

        result.StateCode.ShouldBe(PtnEvidenceStateCodes.Unavailable);
        result.Rows.ShouldBeEmpty();
    }

    // Servisi projection yuzeyi ve repository-native validator'larla kurar.
    private static DatabaseOracleAppService CreateService(IProjectionAppService projection) =>
        new(
            Substitute.For<IDatabaseAssertionAppService>(),
            projection,
            Substitute.For<IAssertionDerivabilityAppService>(),
            new DatabaseOracleManager(),
            new DatabaseAssertionRequestDtoValidator(),
            new DatabaseAssertionBatchRequestDtoValidator(),
            new ProjectionRequestDtoValidator(),
            new DatabaseDerivabilityRequestDtoValidator());
}
