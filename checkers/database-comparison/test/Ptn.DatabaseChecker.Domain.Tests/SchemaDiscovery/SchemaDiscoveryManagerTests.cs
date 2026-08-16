using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Secrets;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Managers.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Secrets;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.DatabaseChecker.SchemaDiscovery;

// islevi: Sema muhru ucunun katalog kapisini ve bilgi alani damgasini dogrular.
// sistemdeki gorevi: Katalogda olmayan sema adinin sessizce atlanip eksik ama gecerli gorunen bir muhur uretmesini engeller.
public class SchemaDiscoveryManagerTests
{
    private static readonly DateTime FixedNow = new(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Schema_Outside_The_Catalog_Should_Fail_Before_The_Snapshot_Is_Read()
    {
        var manager = CreateManager(out var repository, out var connection);

        var exception = await Should.ThrowAsync<BusinessException>(
            () => manager.ReadFingerprintAsync(connection, ["public", "ghost"]));

        exception.Code.ShouldBe(ComparisonExceptionCodes.SchemaNotFound);
        repository.SnapshotWasRead.ShouldBeFalse();
    }

    [Fact]
    public async Task Catalog_Verified_Schema_Should_Produce_A_Stamped_Seal()
    {
        var manager = CreateManager(out _, out var connection);

        var result = await manager.ReadFingerprintAsync(connection, ["public"]);

        result.SnapshotFingerprint.Length.ShouldBe(SchemaFingerprintConsts.FingerprintLength);
        result.AlgorithmCode.ShouldBe(SchemaFingerprintConsts.AlgorithmCode);
        result.Tables.Single().Name.ShouldBe("public.customers");
        result.ComputedAt.ShouldBe(FixedNow);
    }

    [Fact]
    public async Task Describe_Table_Should_Attach_Deterministic_Lint_Warnings()
    {
        var manager = CreateManager(out _, out var connection);

        var result = await manager.DescribeTableAsync(connection, "public", "customers");

        result.LintWarnings.Select(item => item.WarningCode).ShouldBe(
        [
            SchemaLintWarningCodes.MissingPrimaryKey,
            SchemaLintWarningCodes.MissingUniqueKey,
            SchemaLintWarningCodes.GeneratedColumn
        ]);
        result.LintWarnings[2].ColumnName.ShouldBe("id");
    }

    // islevi: Manager'i katalog okuyucusu, saat ve saf hesaplayici bagimliliklariyla kurar.
    private static SchemaDiscoveryManager CreateManager(
        out SchemaDiscoveryRepositoryStub repository,
        out DatabaseConnection connection)
    {
        repository = CreateRepository();
        var resolver = Substitute.For<IEngineComponentResolver<IDatabaseSchemaDiscoveryRepository>>();
        resolver.Resolve(DatabaseEngineCodes.PostgreSql).Returns(repository);
        var manager = new SchemaDiscoveryManager
        {
            LazyServiceProvider = CreateServiceProvider(resolver)
        };
        connection = CreateConnection();
        return manager;
    }

    // islevi: Yalniz public semasini bildiren ve tek tabloluk fotograf donduren katalog okuyucusunu kurar.
    private static SchemaDiscoveryRepositoryStub CreateRepository()
        => new(CreateSnapshot());

    // islevi: Manager'in lazy cozdugu secret, motor, saat ve hesaplayici bagimliliklarini tek saglayicida toplar.
    private static IAbpLazyServiceProvider CreateServiceProvider(
        IEngineComponentResolver<IDatabaseSchemaDiscoveryRepository> resolver)
    {
        var secretProvider = Substitute.For<ISecretProvider>();
        secretProvider.GetDatabaseCredentialAsync(Arg.Any<string>())
            .Returns(new DatabaseCredentialModel { Username = "reader", Password = "secret" });
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(FixedNow);
        var serviceProvider = Substitute.For<IAbpLazyServiceProvider>();
        serviceProvider.LazyGetRequiredService<DatabaseConnectionInfoFactory>()
            .Returns(new DatabaseConnectionInfoFactory(secretProvider));
        serviceProvider.LazyGetRequiredService<IEngineComponentResolver<IDatabaseSchemaDiscoveryRepository>>()
            .Returns(resolver);
        serviceProvider.LazyGetRequiredService<SchemaFingerprintCalculator>()
            .Returns(new SchemaFingerprintCalculator(new SchemaDefinitionNormalizer()));
        serviceProvider.LazyGetRequiredService<DatabaseLintManager>()
            .Returns(new DatabaseLintManager());
        serviceProvider.LazyGetRequiredService<IClock>().Returns(clock);
        return serviceProvider;
    }

    // islevi: Muhur akisi icin runtime PostgreSQL baglanti entity'si kurar.
    private static DatabaseConnection CreateConnection()
        => new(Guid.NewGuid())
        {
            Host = "localhost",
            Port = 5432,
            DatabaseName = "assurance",
            VaultSecretPath = "test/reader",
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };

    // islevi: Katalog kapisi gectikten sonra muhurlenecek tek tabloluk fotografi kurar.
    private static SchemaSnapshotModel CreateSnapshot()
        => new()
        {
            EngineCode = DatabaseEngineCodes.PostgreSql,
            DatabaseName = "assurance",
            CollectedAt = FixedNow,
            Tables =
            [
                new SchemaTableModel
                {
                    Schema = "public",
                    Name = "customers",
                    Columns =
                    [
                        new SchemaColumnModel
                        {
                            Name = "id",
                            Ordinal = 1,
                            RawDataType = "integer",
                            CanonicalDataType = CanonicalDataTypeCodes.Integer,
                            IsGenerated = true
                        }
                    ]
                }
            ]
        };

    // islevi: Default interface method proxy sinirina girmeden hedefli ve tam snapshot okumalarini testte kaydeder.
    private sealed class SchemaDiscoveryRepositoryStub : IDatabaseSchemaDiscoveryRepository
    {
        private readonly SchemaSnapshotModel _snapshot;

        public SchemaDiscoveryRepositoryStub(SchemaSnapshotModel snapshot)
        {
            _snapshot = snapshot;
        }

        public string EngineCode => DatabaseEngineCodes.PostgreSql;
        public bool SnapshotWasRead { get; private set; }

        public Task<List<DatabaseSchemaModel>> GetSchemasAsync(
            DatabaseConnectionInfo info,
            CancellationToken cancellationToken = default)
            => Task.FromResult<List<DatabaseSchemaModel>>([new() { Name = "public" }]);

        public Task<List<DatabaseSchemaObjectModel>> GetObjectsAsync(
            DatabaseConnectionInfo info,
            string schemaName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new List<DatabaseSchemaObjectModel>());

        public Task<SchemaSnapshotModel> ReadSnapshotAsync(
            DatabaseConnectionInfo info,
            List<string> schemaNames,
            CancellationToken cancellationToken = default)
        {
            SnapshotWasRead = true;
            return Task.FromResult(_snapshot);
        }

        public Task<SchemaSnapshotModel> ReadSnapshotAsync(
            DatabaseConnectionInfo info,
            List<string> schemaNames,
            string tableName,
            CancellationToken cancellationToken = default)
        {
            SnapshotWasRead = true;
            return Task.FromResult(_snapshot);
        }
    }
}
