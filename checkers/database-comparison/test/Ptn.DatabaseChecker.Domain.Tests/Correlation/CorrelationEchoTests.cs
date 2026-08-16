using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Localization;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.Diagnosis;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Correlation;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.DatabaseChecker.Correlation;

// islevi: Assertion ve teshis manager'larinin correlation nesnesini sonucuna aynen yansittigini dogrular.
// sistemdeki gorevi: Echo davranisinin AppService'e veya konumsal batch eslesmesine kaymasini regresyona karsi korur.
public sealed class CorrelationEchoTests
{
    // islevi: Row, count ve absent use-case'lerinin kendi correlation referanslarini aynen dondurdugunu dogrular.
    [Fact]
    public async Task Assertion_Use_Cases_Should_Echo_Their_Own_Correlation()
    {
        var (manager, dataManager, connection) = CreateAssertionFixture();
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
            .Returns(CreateObservation(1), CreateObservation(1), CreateObservation(0));
        var row = CreateRequest(connection.Id, "row");
        var count = CreateRequest(connection.Id, "count");
        var absent = CreateRequest(connection.Id, "absent");

        var rowResult = await manager.AssertRowAsync(connection, row);
        var countResult = await manager.AssertCountAsync(connection, count);
        var absentResult = await manager.AssertAbsentAsync(connection, absent);

        rowResult.Correlation.ShouldBeSameAs(row.Correlation);
        countResult.Correlation.ShouldBeSameAs(count.Correlation);
        absentResult.Correlation.ShouldBeSameAs(absent.Correlation);
    }

    // islevi: Correlation verilmediginde assertion sonucunun null kalip mevcut davranisi korudugunu dogrular.
    [Fact]
    public async Task Assertion_Result_Should_Keep_Null_Correlation()
    {
        var (manager, dataManager, connection) = CreateAssertionFixture();
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
            .Returns(CreateObservation(1));
        var request = CreateRequest(connection.Id, null);

        var result = await manager.AssertRowAsync(connection, request);

        result.Correlation.ShouldBeNull();
    }

    // islevi: Diagnosis manager'inin signal correlation referansini rapora aynen yansittigini dogrular.
    [Fact]
    public async Task Diagnosis_Should_Echo_The_Signal_Correlation()
    {
        var connection = CreateConnection();
        var manager = CreateDiagnosisManager(connection);
        var correlation = new CorrelationRef { TraceId = new string('a', 32), StepKey = "diagnosis" };
        var signal = new FailureSignal
        {
            Assertion = new FailureSignal.AssertionFailureSignal(),
            Correlation = correlation
        };

        var report = await manager.DiagnoseAsync(connection, signal);

        report.Correlation.ShouldBeSameAs(correlation);
    }

    // islevi: Assertion echo testleri icin gercek matcher/redactor ile mock data manager, setting ve saati birlestirir.
    private static (RowAssertionManager Manager, DatabaseDataComparisonManager DataManager, DatabaseConnection Connection)
        CreateAssertionFixture()
    {
        var settings = CreateSettings();
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
        var connection = CreateConnection();
        return (manager, dataManager, connection);
    }

    // islevi: Diagnosis echo testi icin bos kural/probe akisini ve katalog resolver test double'ini birlestirir.
    private static DiagnosisManager CreateDiagnosisManager(DatabaseConnection connection)
    {
        var extractor = Substitute.For<IFailureIdentityExtractor>();
        extractor.Extract(Arg.Any<FailureSignal>()).Returns(new FailureIdentity
        {
            EngineCode = connection.Engine.Code
        });
        var extractorResolver = Substitute.For<IEngineComponentResolver<IFailureIdentityExtractor>>();
        extractorResolver.Resolve(connection.Engine.Code).Returns(extractor);
        var contextResolver = Substitute.For<FailureContextResolver>(null!, null!, new FindingValueRedactor());
        contextResolver.ResolveAsync(
                connection,
                Arg.Any<FailureSignal>(),
                Arg.Any<FailureIdentity>(),
                Arg.Any<ValueRetentionPolicy>(),
                Arg.Any<CancellationToken>())
            .Returns(new ResolvedFailureContext());
        var settings = CreateSettings();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 8, 14, 0, 0, 0, DateTimeKind.Utc));
        var localizer = Substitute.For<IStringLocalizer<DatabaseCheckerResource>>();
        localizer[Arg.Any<string>()].Returns(call =>
            new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        return new DiagnosisManager(
            extractorResolver,
            contextResolver,
            new ValueRetentionPolicyResolver(settings),
            new ProbeBudgetManager(settings, clock, Array.Empty<IDiagnosisProbe>()),
            new HypothesisRankingManager(localizer),
            Array.Empty<IDiagnosisRule>());
    }

    // islevi: Test manager'lari icin varsayilan degerleri kullanan ABP setting provider'i kurar.
    private static ISettingProvider CreateSettings()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return settings;
    }

    // islevi: Echo testleri icin PostgreSQL engine navigation'i yuklu baglanti kurar.
    private static DatabaseConnection CreateConnection()
        => new(Guid.NewGuid())
        {
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };

    // islevi: Assertion testleri icin PK ve status kolonu bulunan katalog modelini kurar.
    private static TableDataStructureModel CreateStructure()
        => new()
        {
            SchemaName = "public",
            TableName = "orders",
            ColumnNames = ["id", "status"],
            PrimaryKeyColumns = ["id"],
            UniqueKeyColumnSets = [["id"]],
            Columns =
            [
                new TableDataColumnModel { Name = "id", CanonicalDataTypeCode = CanonicalDataTypeCodes.Integer },
                new TableDataColumnModel { Name = "status", CanonicalDataTypeCode = CanonicalDataTypeCodes.String }
            ]
        };

    // islevi: Assertion testleri icin istenen satir sayisinda Ready degerli observation kurar.
    private static RowAssertionObservation CreateObservation(long rowCount)
        => new()
        {
            RowCount = rowCount,
            Rows = rowCount == 0
                ? []
                :
                [
                    new TableDataRowModel
                    {
                        Values = new Dictionary<string, string?>
                        {
                            ["id"] = "42",
                            ["status"] = "Ready"
                        }
                    }
                ]
        };

    // islevi: Assertion echo testleri icin Exactly-one ve opsiyonel correlation tasiyan request kurar.
    private static RowAssertionRequest CreateRequest(Guid connectionId, string? stepKey)
        => new()
        {
            ConnectionId = connectionId,
            SchemaName = "public",
            TableName = "orders",
            KeyValues = new Dictionary<string, string?> { ["id"] = "42" },
            Expectations =
            [
                new ColumnExpectation
                {
                    ColumnName = "status",
                    MatcherKindCode = MatcherKindCodes.Equals,
                    ExpectedValue = "Ready"
                }
            ],
            Cardinality = new CardinalityExpectation
            {
                KindCode = CardinalityKindCodes.Exactly,
                ExpectedCount = 1
            },
            Correlation = stepKey is null
                ? null
                : new CorrelationRef { TraceId = new string('a', 32), StepKey = stepKey }
        };
}
