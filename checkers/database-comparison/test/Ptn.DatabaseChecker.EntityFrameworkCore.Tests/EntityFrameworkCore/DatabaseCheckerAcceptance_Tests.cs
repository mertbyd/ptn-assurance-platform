using System.Diagnostics;
using System.Text.Json;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Interface.Runs;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.Diagnosis;
using Ptn.DatabaseChecker.Managers.Diagnosis.Rules;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Ptn.DatabaseChecker.Models.Runs;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;
using Xunit.Abstractions;

namespace Ptn.DatabaseChecker.EntityFrameworkCore;

// islevi: Test Module'un describe -> timeout -> diagnosis -> insert -> batch -> kucuk findings referans akisini tek testte calistirir.
// sistemdeki gorevi: KBP-706 once/sonra kiyasina sure, hedef I/O cagrisi ve cevap byte olcumlerini mevcut ABP EF Core test altyapisinda verir.
public class DatabaseCheckerAcceptance_Tests : DatabaseCheckerEntityFrameworkCoreTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly IComparisonRunRepository _runRepository;
    private readonly IRepository<DatabaseEngine, Guid> _engineRepository;
    private readonly IRepository<ComparisonType, Guid> _comparisonTypeRepository;
    private readonly IRepository<ComparisonRunStatus, Guid> _statusRepository;
    private readonly IRepository<DatabaseConnection, Guid> _connectionRepository;
    private readonly IGuidGenerator _guidGenerator;

    public DatabaseCheckerAcceptance_Tests(ITestOutputHelper output)
    {
        _output = output;
        _runRepository = GetRequiredService<IComparisonRunRepository>();
        _engineRepository = GetRequiredService<IRepository<DatabaseEngine, Guid>>();
        _comparisonTypeRepository = GetRequiredService<IRepository<ComparisonType, Guid>>();
        _statusRepository = GetRequiredService<IRepository<ComparisonRunStatus, Guid>>();
        _connectionRepository = GetRequiredService<IRepository<DatabaseConnection, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Test_Module_Reference_Flow_Should_Remain_Bounded_And_Observable()
    {
        var elapsed = Stopwatch.StartNew();
        var state = new AcceptanceState();
        var connection = CreateConnection();
        var structure = CreateStructure();
        DescribeTable(structure).TableName.ShouldBe("orders");
        var assertionManager = CreateAssertionManager(state, structure);
        var timedOut = await assertionManager.AssertAsync(connection, CreateRequest(connection.Id));
        timedOut.OutcomeCode.ShouldBe(AssertionOutcomeCodes.TimedOut);
        var report = await DiagnoseAsync(connection, structure, state);
        AssertConfirmedMissingRow(report);
        state.RowExists = true;
        var batch = await AssertInsertedRowAsync(assertionManager, connection);
        batch.ShouldAllBe(result => result.OutcomeCode == AssertionOutcomeCodes.Passed);
        var page = await ReadPersistedFindingPageAsync();
        var reportBytes = report.MeasureUtf8Bytes();
        var findingBytes = JsonSerializer.SerializeToUtf8Bytes(page).Length;
        reportBytes.ShouldBeLessThan(FailureSourceKindCodes.Report.MaxUtf8Bytes);
        findingBytes.ShouldBeLessThan(ComparisonRunConsts.DefaultFindingResponseBytes);
        elapsed.Stop();
        WriteMetrics(state.TargetIoCalls, reportBytes, findingBytes, elapsed.ElapsedMilliseconds);
    }

    // islevi: Hedefli snapshot'in dar tablo modelini DescribeTable sozlesmesine cevirir.
    private static TableDescriptionModel DescribeTable(TableDataStructureModel structure)
    {
        var table = CreateSchemaTable(structure);
        return SchemaDiscoveryManager.BuildTableDescription(
            new SchemaSnapshotModel { Tables = new List<SchemaTableModel> { table } },
            table);
    }

    // islevi: Stateful hedef gozlemini gercek assertion manager polling akisi altinda calistirir.
    private static RowAssertionManager CreateAssertionManager(
        AcceptanceState state,
        TableDataStructureModel structure)
    {
        var dataManager = Substitute.For<DatabaseDataComparisonManager>();
        dataManager.ResolveAssertionStructureAsync(
                Arg.Any<DatabaseConnection>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => { state.TargetIoCalls++; return structure; });
        dataManager.ReadAssertionObservationAsync(
                Arg.Any<DatabaseConnection>(), Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(), Arg.Any<int>(), true, Arg.Any<CancellationToken>())
            .Returns(_ => { state.TargetIoCalls++; return state.RowExists ? CreateObservation() : new RowAssertionObservation(); });
        var settings = CreateSettingProvider();
        return new RowAssertionManager(
            dataManager,
            new ValueMatcherEvaluator(),
            new AssertionSettingsResolver(settings),
            new ValueRetentionPolicyResolver(settings),
            new FindingValueRedactor(),
            CreateAdvancingClock());
    }

    // islevi: Gercek diagnosis orkestrasyonu, RowNeverCreated kurali ve missing probe kanitini birlestirir.
    private async Task<DiagnosisReport> DiagnoseAsync(
        DatabaseConnection connection,
        TableDataStructureModel structure,
        AcceptanceState state)
    {
        var signal = CreateFailureSignal(connection.Id);
        var extractor = Substitute.For<IFailureIdentityExtractor>();
        extractor.EngineCode.Returns(DatabaseEngineCodes.PostgreSql);
        extractor.Extract(signal).Returns(FailureIdentity.FromAssertion(DatabaseEngineCodes.PostgreSql, signal.Assertion!));
        var extractorResolver = Substitute.For<IEngineComponentResolver<IFailureIdentityExtractor>>();
        extractorResolver.Resolve(DatabaseEngineCodes.PostgreSql).Returns(extractor);
        var contextResolver = CreateContextResolver(structure, state);
        var settings = CreateSettingProvider();
        var manager = new DiagnosisManager(
            extractorResolver,
            contextResolver,
            new ValueRetentionPolicyResolver(settings),
            CreateProbeBudgetManager(settings, state),
            GetRequiredService<HypothesisRankingManager>(),
            new IDiagnosisRule[] { new RowNeverCreatedRule() });
        return await manager.DiagnoseAsync(connection, signal);
    }

    // islevi: Katalogda dogrulanmis eksik-satir context'ini diagnosis manager'a verir.
    private static FailureContextResolver CreateContextResolver(
        TableDataStructureModel structure,
        AcceptanceState state)
    {
        var resolver = Substitute.For<FailureContextResolver>(
            Substitute.For<SchemaDiscoveryManager>(),
            Substitute.For<DatabaseDataComparisonManager>(),
            new FindingValueRedactor());
        resolver.ResolveAsync(
                Arg.Any<DatabaseConnection>(), Arg.Any<FailureSignal>(), Arg.Any<FailureIdentity>(),
                Arg.Any<ValueRetentionPolicy>(), Arg.Any<CancellationToken>())
            .Returns(_ => { state.TargetIoCalls++; return CreateResolvedContext(structure); });
        return resolver;
    }

    // islevi: RowExists probe'unu missing kanitiyla tamamlayan gercek butce yoneticisini kurar.
    private static ProbeBudgetManager CreateProbeBudgetManager(
        ISettingProvider settings,
        AcceptanceState state)
    {
        var probe = Substitute.For<IDiagnosisProbe>();
        probe.ProbeKindCode.Returns(ProbeKindCodes.RowExists);
        probe.RunAsync(
                Arg.Any<DatabaseConnection>(), Arg.Any<ProbeRequest>(),
                Arg.Any<ValueRetentionPolicy>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                state.TargetIoCalls++;
                return CreateMissingEvidence(call.ArgAt<ProbeRequest>(1));
            });
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc));
        return new ProbeBudgetManager(settings, clock, new[] { probe });
    }

    // islevi: Insert sonrasi iki assertion'i tek batch manager cagrisinda calistirir.
    private static async Task<List<RowAssertionResult>> AssertInsertedRowAsync(
        RowAssertionManager manager,
        DatabaseConnection connection)
    {
        var requests = new List<RowAssertionRequest>
        {
            CreateRequest(connection.Id),
            CreateRequest(connection.Id)
        };
        return await manager.AssertBatchAsync(new List<DatabaseConnection> { connection }, requests);
    }

    // islevi: Owned JSON bulguyu mevcut SQLite ABP repository altyapisina kaydedip sayfali okur.
    private async Task<FindingPageModel> ReadPersistedFindingPageAsync()
    {
        var runId = await SeedFindingRunAsync();
        var run = await WithUnitOfWorkAsync(() => _runRepository.FindWithDetailsAsync(runId));
        var difference = run!.Findings.SchemaDifferences.Single();
        return new FindingPageModel
        {
            TotalCount = 1,
            Items = new List<FindingReadModel> { MapFinding(difference) }
        };
    }

    // islevi: SQLite kabul fixture'indaki kalici owned JSON farkini tuketici finding sayfasina cevirir.
    private static FindingReadModel MapFinding(SchemaDifferenceModel difference)
        => new()
        {
            Fingerprint = difference.Fingerprint,
            SeverityCode = difference.SeverityCode,
            KindCode = difference.KindCode,
            ObjectTypeCode = difference.ObjectTypeCode,
            SchemaName = difference.SchemaName,
            ObjectName = difference.ObjectName,
            TableName = difference.ObjectName,
            ConfidenceCode = difference.ConfidenceCode
        };

    // islevi: Sayfali bulgu sorgusu icin gerekli lookup, baglanti ve tek-finding run grafigini olusturur.
    private async Task<Guid> SeedFindingRunAsync()
    {
        var engineId = await GetLookupIdAsync(_engineRepository, DatabaseEngineCodes.PostgreSql);
        var comparisonTypeId = await GetLookupIdAsync(_comparisonTypeRepository, ComparisonTypeCodes.SchemaOnly);
        var statusId = await GetLookupIdAsync(_statusRepository, ComparisonRunStatusCodes.Completed);
        return await WithUnitOfWorkAsync(async () =>
        {
            var source = CreateStoredConnection(engineId, "acceptance-source");
            var target = CreateStoredConnection(engineId, "acceptance-target");
            await _connectionRepository.InsertManyAsync(new[] { source, target }, autoSave: true);
            var run = CreateFindingRun(source.Id, target.Id, comparisonTypeId, statusId);
            await _runRepository.InsertAsync(run, autoSave: true);
            return run.Id;
        });
    }

    // islevi: Koduyla seeded lookup kimligini mevcut repository'den okur.
    private Task<Guid> GetLookupIdAsync<TEntity>(IRepository<TEntity, Guid> repository, string code)
        where TEntity : LookupEntity
        => WithUnitOfWorkAsync(async () => (await repository.GetListAsync(item => item.Code == code)).Single().Id);

    // islevi: Kabul bulgu sayfasina tek kararli schema farki tasiyan run olusturur.
    private ComparisonRun CreateFindingRun(Guid sourceId, Guid targetId, Guid comparisonTypeId, Guid statusId)
        => new(_guidGenerator.Create())
        {
            SourceConnectionId = sourceId,
            TargetConnectionId = targetId,
            ComparisonTypeId = comparisonTypeId,
            StatusId = statusId,
            SchemaDifferenceCount = 1,
            Findings = new ComparisonFindings
            {
                SchemaDifferences = new List<SchemaDifferenceModel>
                {
                    new()
                    {
                        Fingerprint = new string('A', 64),
                        SeverityCode = DifferenceSeverityCodes.Breaking,
                        SchemaName = "public",
                        ObjectName = "orders",
                        ObjectTypeCode = SchemaObjectTypeCodes.Table,
                        KindCode = DifferenceKindCodes.OnlyInSource,
                        ConfidenceCode = ComparisonConfidenceCodes.Exact
                    }
                }
            }
        };

    // islevi: SQLite run grafigi icin en az alanli kalici baglanti olusturur.
    private DatabaseConnection CreateStoredConnection(Guid engineId, string name)
        => new(_guidGenerator.Create())
        {
            EngineId = engineId,
            Name = name,
            Host = "localhost",
            Port = 5432,
            DatabaseName = name,
            VaultSecretPath = $"test/{name}",
            IsActive = true
        };

    // islevi: Kabul akisi icin runtime PostgreSQL baglanti entity'si olusturur.
    private static DatabaseConnection CreateConnection()
        => new(Guid.NewGuid())
        {
            DatabaseName = "acceptance",
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };

    // islevi: Orders tablosunun assertion ve describe tarafinda paylasilan yapisini kurar.
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

    // islevi: Assertion yapisini schema discovery tablo modeline ceviren test fixture'i kurar.
    private static SchemaTableModel CreateSchemaTable(TableDataStructureModel structure)
        => new()
        {
            Schema = structure.SchemaName,
            Name = structure.TableName,
            Columns = structure.Columns.Select((column, index) => new SchemaColumnModel
            {
                Name = column.Name,
                Ordinal = index + 1,
                CanonicalDataType = column.CanonicalDataTypeCode
            }).ToList(),
            Indexes = new List<SchemaIndexModel>
            {
                new() { Name = "pk_orders", IsUnique = true, IsPrimaryKey = true, Columns = new List<string> { "id" } }
            }
        };

    // islevi: Exactly-one status assertion istegini kurar.
    private static RowAssertionRequest CreateRequest(Guid connectionId)
        => new()
        {
            ConnectionId = connectionId,
            SchemaName = "public",
            TableName = "orders",
            KeyValues = new Dictionary<string, string?> { ["id"] = "42" },
            Expectations = new List<ColumnExpectation>
            {
                new() { ColumnName = "status", MatcherKindCode = MatcherKindCodes.Equals, ExpectedValue = "Ready" }
            },
            Cardinality = new CardinalityExpectation { KindCode = CardinalityKindCodes.Exactly, ExpectedCount = 1 },
            TimeoutMs = 5,
            PollIntervalMs = 1
        };

    // islevi: Insert sonrasi bulunan Ready satirini hedef observation olarak kurar.
    private static RowAssertionObservation CreateObservation()
        => new()
        {
            RowCount = 1,
            Rows = new List<TableDataRowModel>
            {
                new() { Values = new Dictionary<string, string?> { ["id"] = "42", ["status"] = "Ready" } }
            }
        };

    // islevi: Timeout assertion sonucunu diagnosis failure sinyaline cevirir.
    private static FailureSignal CreateFailureSignal(Guid connectionId)
        => new()
        {
            ConnectionId = connectionId,
            Assertion = new FailureSignal.AssertionFailureSignal
            {
                SchemaName = "public",
                TableName = "orders",
                OutcomeCode = AssertionOutcomeCodes.TimedOut,
                KeyValues = new Dictionary<string, string?> { ["id"] = "42" }
            }
        };

    // islevi: RowNeverCreated kuralina katalogda dogrulanmis unique anahtar context'i verir.
    private static ResolvedFailureContext CreateResolvedContext(TableDataStructureModel structure)
        => new()
        {
            EngineCode = DatabaseEngineCodes.PostgreSql,
            Location = new ObjectReference { SchemaName = "public", TableName = "orders", IsCatalogVerified = true },
            RowWasReportedMissing = true,
            RowTimedOut = true,
            TargetStructure = structure,
            TargetKeyValues = new Dictionary<string, string?> { ["id"] = "42" },
            IdentityKeyValues = new Dictionary<string, string?> { ["id"] = "42" }
        };

    // islevi: RowExists probe sonucunu RowNeverCreated icin missing kanitina cevirir.
    private static ProbeEvidence CreateMissingEvidence(ProbeRequest request)
        => new()
        {
            ProbeKindCode = ProbeKindCodes.RowExists,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = ProbeKindCodes.Facts.Missing,
            ObservedCount = 0
        };

    // islevi: Kabul akisindaki tum tenant-aware ayarlari urun varsayilanlarina dusuren provider'i kurar.
    private static ISettingProvider CreateSettingProvider()
    {
        var provider = Substitute.For<ISettingProvider>();
        provider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        provider.GetOrNullAsync(DatabaseCheckerSettings.Assertion.MinPollIntervalMs).Returns("1");
        return provider;
    }

    // islevi: Assertion timeout'ini deterministik ve hizli ilerleten ABP saatini kurar.
    private static IClock CreateAdvancingClock()
    {
        var clock = Substitute.For<IClock>();
        var now = new DateTime(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc);
        clock.Now.Returns(_ => now = now.AddMilliseconds(10));
        return clock;
    }

    // islevi: Diagnosis sonucunda RowNeverCreated hipotezinin Confirmed oldugunu dogrular.
    private static void AssertConfirmedMissingRow(DiagnosisReport report)
    {
        var hypothesis = report.Hypotheses.Single();
        hypothesis.HypothesisKindCode.ShouldBe(HypothesisKindCodes.RowNeverCreated);
        hypothesis.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Confirmed);
    }

    // islevi: Kabul akisi olcumlerini once/sonra kiyasinda okunabilir tek satirda yazar.
    private void WriteMetrics(int targetIoCalls, int reportBytes, int findingBytes, long durationMs)
        => _output.WriteLine(
            $"steps=5; target_io_calls={targetIoCalls}; findings_round_trips={ComparisonRunConsts.FindingQueryRoundTripCount}; " +
            $"report_bytes={reportBytes}; findings_bytes={findingBytes}; duration_ms={durationMs}");

    private sealed class AcceptanceState
    {
        public bool RowExists { get; set; }
        public int TargetIoCalls { get; set; }
    }
}
