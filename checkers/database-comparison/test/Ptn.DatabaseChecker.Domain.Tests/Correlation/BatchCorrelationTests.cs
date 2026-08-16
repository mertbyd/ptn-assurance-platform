using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Correlation;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.DatabaseChecker.Correlation;

// islevi: Batch assertion sonuclarinin oge korelasyonunu ve tam sonuc sayisi garantisini dogrular.
// sistemdeki gorevi: Sirasi degisen veya oge dusuren sonuclarin sessizce baska senaryo adimina yazilmasini engeller.
public sealed class BatchCorrelationTests
{
    // islevi: Uc istekli batch sonucunda her ogenin kendi StepKey referansini aynen tasidigini dogrular.
    [Fact]
    public async Task Batch_Results_Should_Carry_Their_Own_Step_Keys()
    {
        var (manager, dataManager, connection, _) = CreateFixture();
        dataManager.ResolveAssertionStructureAsync(
                connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        dataManager.ReadAssertionObservationAsync(
                connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateObservation());
        var requests = Enumerable.Range(1, 3)
            .Select(index => CreateRequest(connection.Id, $"step-{index}"))
            .ToList();

        var results = await manager.AssertBatchAsync([connection], requests);

        results.Select(result => result.Correlation!.StepKey)
            .ShouldBe(["step-1", "step-2", "step-3"]);
        results.Zip(requests).ShouldAllBe(pair =>
            ReferenceEquals(pair.First.Correlation, pair.Second.Correlation));
    }

    // islevi: Eksik batch sonucunda manager'in kararli kodla firlatip kismi liste dondurmedigini dogrular.
    [Fact]
    public async Task Batch_Result_Count_Mismatch_Should_Throw_Instead_Of_Returning_Partial_Results()
    {
        var (_, dataManager, connection, settings) = CreateFixture();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 8, 14, 0, 0, 0, DateTimeKind.Utc));
        var manager = new BatchResultMismatchRowAssertionManager(
            dataManager,
            new ValueMatcherEvaluator(),
            new AssertionSettingsResolver(settings),
            new ValueRetentionPolicyResolver(settings),
            new FindingValueRedactor(),
            clock,
            [new RowAssertionResult(), new RowAssertionResult()]);
        var requests = Enumerable.Range(1, 3)
            .Select(index => CreateRequest(connection.Id, $"step-{index}"))
            .ToList();

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.AssertBatchAsync([connection], requests));

        exception.Code.ShouldBe(AssertionExceptionCodes.Validation.BatchResultCountMismatch);
    }

    // islevi: Batch testleri icin manager, mock data reader, PostgreSQL baglanti ve ortak setting provider'i kurar.
    private static (
        RowAssertionManager Manager,
        DatabaseDataComparisonManager DataManager,
        DatabaseConnection Connection,
        ISettingProvider Settings) CreateFixture()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        settings.GetOrNullAsync(DatabaseCheckerSettings.Assertion.MinPollIntervalMs).Returns("1");
        var dataManager = Substitute.For<DatabaseDataComparisonManager>();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 8, 14, 0, 0, 0, DateTimeKind.Utc));
        var manager = new RowAssertionManager(
            dataManager,
            new ValueMatcherEvaluator(),
            new AssertionSettingsResolver(settings),
            new ValueRetentionPolicyResolver(settings),
            new FindingValueRedactor(),
            clock);
        var connection = new DatabaseConnection(Guid.NewGuid())
        {
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };
        return (manager, dataManager, connection, settings);
    }

    // islevi: Batch testleri icin tek integer PK'li katalog yapisini kurar.
    private static TableDataStructureModel CreateStructure()
        => new()
        {
            SchemaName = "public",
            TableName = "orders",
            ColumnNames = ["id"],
            PrimaryKeyColumns = ["id"],
            UniqueKeyColumnSets = [["id"]],
            Columns =
            [
                new TableDataColumnModel { Name = "id", CanonicalDataTypeCode = CanonicalDataTypeCodes.Integer }
            ]
        };

    // islevi: Batch testleri icin tek satirli ve deger okumayan observation kurar.
    private static RowAssertionObservation CreateObservation()
        => new()
        {
            RowCount = 1
        };

    // islevi: Batch testleri icin kendi StepKey'i ve Exactly-one cardinality'si bulunan request kurar.
    private static RowAssertionRequest CreateRequest(Guid connectionId, string stepKey)
        => new()
        {
            ConnectionId = connectionId,
            SchemaName = "public",
            TableName = "orders",
            KeyValues = new Dictionary<string, string?> { ["id"] = "42" },
            Cardinality = new CardinalityExpectation
            {
                KindCode = CardinalityKindCodes.Exactly,
                ExpectedCount = 1
            },
            Correlation = new CorrelationRef
            {
                TraceId = new string('a', 32),
                StepKey = stepKey
            }
        };
}
