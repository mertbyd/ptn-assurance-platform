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
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: RowAssertionManager'in polling, katalog guvenligi, matcher failure ve retention davranislarini dogrular.
// sistemdeki gorevi: Test Module oracle cekirdeginin hedef sorguyu yalniz benzersiz ve mevcut kolonlarda calistirdigini regresyona karsi korur.
public class RowAssertionManager_Tests
{
    [Fact]
    public async Task Existing_Row_With_Correct_Expectation_Should_Pass_First_Attempt()
    {
        var fixture = CreateFixture();
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        fixture.DataManager.ReadAssertionObservationAsync(
                fixture.Connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(CreateObservation("Ready"));

        var result = await fixture.Manager.AssertAsync(fixture.Connection, CreateRequest("Ready"));

        result.OutcomeCode.ShouldBe(AssertionOutcomeCodes.Passed);
        result.AttemptCount.ShouldBe(1);
    }

    [Fact]
    public async Task Missing_Row_Should_Time_Out_After_Multiple_Attempts()
    {
        var fixture = CreateFixture();
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        fixture.DataManager.ReadAssertionObservationAsync(
                fixture.Connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(new RowAssertionObservation());
        var request = CreateRequest("Ready");
        request.TimeoutMs = 5;
        request.PollIntervalMs = 1;

        var result = await fixture.Manager.AssertAsync(fixture.Connection, request);

        result.OutcomeCode.ShouldBe(AssertionOutcomeCodes.TimedOut);
        result.AttemptCount.ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task Row_Appearing_On_Second_Attempt_Should_Pass()
    {
        var fixture = CreateFixture();
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        fixture.DataManager.ReadAssertionObservationAsync(
                fixture.Connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(new RowAssertionObservation(), CreateObservation("Ready"));
        var request = CreateRequest("Ready");
        request.TimeoutMs = 100;
        request.PollIntervalMs = 1;

        var result = await fixture.Manager.AssertAsync(fixture.Connection, request);

        result.OutcomeCode.ShouldBe(AssertionOutcomeCodes.Passed);
        result.AttemptCount.ShouldBe(2);
    }

    [Fact]
    public async Task Non_Unique_Key_Should_Not_Run_Data_Query()
    {
        var fixture = CreateFixture();
        var structure = CreateStructure();
        structure.UniqueKeyColumnSets.Clear();
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(structure);

        var result = await fixture.Manager.AssertAsync(fixture.Connection, CreateRequest("Ready"));

        result.OutcomeCode.ShouldBe(AssertionOutcomeCodes.KeyNotUnique);
        await fixture.DataManager.DidNotReceiveWithAnyArgs().ReadAssertionObservationAsync(
            default!, default!, default!, default, default, default);
    }

    [Fact]
    public async Task Missing_Column_Should_Not_Run_Data_Query()
    {
        var fixture = CreateFixture();
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        var request = CreateRequest("Ready");
        request.Expectations[0].ColumnName = "missing";

        var result = await fixture.Manager.AssertAsync(fixture.Connection, request);

        result.OutcomeCode.ShouldBe(AssertionOutcomeCodes.ColumnNotFound);
        await fixture.DataManager.DidNotReceiveWithAnyArgs().ReadAssertionObservationAsync(
            default!, default!, default!, default, default, default);
    }

    [Fact]
    public async Task Different_Value_Should_Return_Failed_Expectation()
    {
        var fixture = CreateFixture(ValueRetentionModeCodes.Full);
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        fixture.DataManager.ReadAssertionObservationAsync(
                fixture.Connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(CreateObservation("Pending"));

        var result = await fixture.Manager.AssertAsync(fixture.Connection, CreateRequest("Ready"));

        result.OutcomeCode.ShouldBe(AssertionOutcomeCodes.ValueMismatch);
        result.FailedExpectations.ShouldHaveSingleItem();
        result.FailedExpectations[0].ObservedValue.ShouldBe("Pending");
    }

    [Fact]
    public async Task Retention_None_Should_Not_Expose_Source_Value()
    {
        var fixture = CreateFixture();
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        fixture.DataManager.ReadAssertionObservationAsync(
                fixture.Connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(CreateObservation("source-secret"));
        var request = CreateRequest("Ready");
        request.IncludeRowOnFailure = true;

        var result = await fixture.Manager.AssertAsync(fixture.Connection, request);

        result.FailedExpectations.Single().ObservedValue.ShouldBeNull();
        result.RowSummary!.Values.ShouldAllBe(value => value == null);
    }

    [Fact]
    public async Task Independent_Assertions_Should_All_Return_When_One_Fails()
    {
        var fixture = CreateFixture();
        fixture.DataManager.ResolveAssertionStructureAsync(fixture.Connection, "public", "orders", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        fixture.DataManager.ReadAssertionObservationAsync(
                fixture.Connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(
                CreateObservation("Ready"),
                CreateObservation("Pending"),
                CreateObservation("Ready"));

        var results = new List<RowAssertionResult>();
        foreach (var request in Enumerable.Range(0, 3).Select(_ => CreateRequest("Ready")))
        {
            results.Add(await fixture.Manager.AssertAsync(fixture.Connection, request));
        }

        results.Count.ShouldBe(3);
        results.Count(result => result.Passed).ShouldBe(2);
        results.Count(result => !result.Passed).ShouldBe(1);
    }

    [Fact]
    public async Task Cancellation_Should_Stop_Polling_Without_Waiting_For_Timeout()
    {
        var fixture = CreateFixture();
        fixture.DataManager.ResolveAssertionStructureAsync(
                fixture.Connection,
                "public",
                "orders",
                Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        fixture.DataManager.ReadAssertionObservationAsync(
                fixture.Connection,
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<int>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(new RowAssertionObservation());
        var request = CreateRequest("Ready");
        request.TimeoutMs = 1000;
        request.PollIntervalMs = 100;
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(20);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            fixture.Manager.AssertAsync(fixture.Connection, request, cancellation.Token));
    }

    // islevi: Manager testleri icin PK yapisi ve string status kolonu bulunan katalog modelini kurar.
    private static TableDataStructureModel CreateStructure()
        => new()
        {
            SchemaName = "public",
            TableName = "orders",
            ColumnNames = new List<string> { "id", "status" },
            PrimaryKeyColumns = new List<string> { "id" },
            UniqueKeyColumnSets = new List<List<string>> { new() { "id" } },
            Columns = new List<TableDataColumnModel>
            {
                new() { Name = "id", CanonicalDataTypeCode = CanonicalDataTypeCodes.Integer },
                new() { Name = "status", CanonicalDataTypeCode = CanonicalDataTypeCodes.String }
            }
        };

    // islevi: Tek status degerli assertion observation modeli kurar.
    private static RowAssertionObservation CreateObservation(string status)
        => new()
        {
            RowCount = 1,
            Rows = new List<TableDataRowModel>
            {
                new()
                {
                    Values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["id"] = "42",
                        ["status"] = status
                    }
                }
            }
        };

    // islevi: Exactly-one ve status Equals beklentili ortak domain request'ini kurar.
    private static RowAssertionRequest CreateRequest(string expectedStatus)
        => new()
        {
            SchemaName = "public",
            TableName = "orders",
            KeyValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["id"] = "42" },
            Expectations = new List<ColumnExpectation>
            {
                new()
                {
                    ColumnName = "status",
                    MatcherKindCode = MatcherKindCodes.Equals,
                    ExpectedValue = expectedStatus
                }
            },
            Cardinality = new CardinalityExpectation
            {
                KindCode = CardinalityKindCodes.Exactly,
                ExpectedCount = 1
            }
        };

    // islevi: Gercek matcher/redactor ile mock veri manager ve ayar/saat bagimliliklarini birlestiren test fixture'i kurar.
    private static AssertionFixture CreateFixture(string retentionMode = ValueRetentionModeCodes.None)
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.Assertion.MinPollIntervalMs).Returns("1");
        settingProvider.GetOrNullAsync(DatabaseCheckerSettings.DataComparison.ValueRetentionMode).Returns(retentionMode);
        var dataManager = Substitute.For<DatabaseDataComparisonManager>();
        var clock = Substitute.For<IClock>();
        var now = new DateTime(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc);
        clock.Now.Returns(_ => now = now.AddMilliseconds(10));
        var manager = new RowAssertionManager(
            dataManager,
            new ValueMatcherEvaluator(),
            new AssertionSettingsResolver(settingProvider),
            new ValueRetentionPolicyResolver(settingProvider),
            new FindingValueRedactor(),
            clock);
        var connection = new DatabaseConnection(Guid.NewGuid())
        {
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };
        return new AssertionFixture(manager, dataManager, connection);
    }

    // islevi: Testte birlikte kullanilan manager, mock data manager ve connection'i isimli tek degerde tasir.
    private sealed record AssertionFixture(
        RowAssertionManager Manager,
        DatabaseDataComparisonManager DataManager,
        DatabaseConnection Connection);
}
