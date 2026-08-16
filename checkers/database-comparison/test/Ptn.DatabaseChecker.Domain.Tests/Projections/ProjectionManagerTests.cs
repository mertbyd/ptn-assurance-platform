using NSubstitute;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Projections;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Projections;
using Ptn.DatabaseChecker.Interface.Secrets;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Managers.Projections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Projections;
using Ptn.DatabaseChecker.Models.Secrets;
using Ptn.DatabaseChecker.Settings;
using Shouldly;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.DatabaseChecker.Projections;

// islevi: ProjectionManager'in katalog outcome, truncation ve mevcut redaction politikasi davranislarini dogrular.
// sistemdeki gorevi: Katalog gecmeden sorgu acilmamasini, satir tavanini ve ham deger sizintisiz sonucu korur.
public class ProjectionManagerTests
{
    [Fact]
    public async Task Missing_Table_Should_Return_TableNotFound_Without_Query()
    {
        var manager = CreateManager(out var dataManager, out var repository, out var connection);
        dataManager.ResolveAssertionStructureAsync(
                connection, "public", "users", Arg.Any<CancellationToken>())
            .Returns((TableDataStructureModel?)null);

        var result = await manager.ProjectAsync(connection, CreateRequest());

        result.OutcomeCode.ShouldBe(ProjectionOutcomeCodes.TableNotFound);
        await repository.DidNotReceiveWithAnyArgs().ReadProjectionRowsAsync(
            default!, default!, default!, default!, default, default);
    }

    [Fact]
    public async Task Missing_Project_Column_Should_Return_ColumnNotFound_Without_Query()
    {
        var manager = CreateManager(out var dataManager, out var repository, out var connection);
        dataManager.ResolveAssertionStructureAsync(
                connection, "public", "users", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        var request = CreateRequest();
        request.ProjectColumns = ["missing"];

        var result = await manager.ProjectAsync(connection, request);

        result.OutcomeCode.ShouldBe(ProjectionOutcomeCodes.ColumnNotFound);
        await repository.DidNotReceiveWithAnyArgs().ReadProjectionRowsAsync(
            default!, default!, default!, default!, default, default);
    }

    [Fact]
    public async Task Row_Budget_Should_Be_Bounded_And_Explicitly_Truncated()
    {
        var manager = CreateManager(out var dataManager, out var repository, out var connection);
        dataManager.ResolveAssertionStructureAsync(
                connection, "public", "users", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        repository.ReadProjectionRowsAsync(
                Arg.Any<DatabaseConnectionInfo>(),
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<List<string>>(),
                3,
                Arg.Any<CancellationToken>())
            .Returns([Row("one"), Row("two"), Row("three")]);
        var request = CreateRequest();
        request.MaxRows = 2;

        var result = await manager.ProjectAsync(connection, request);

        result.OutcomeCode.ShouldBe(ProjectionOutcomeCodes.Truncated);
        result.Truncated.ShouldBeTrue();
        result.ObservedRowCount.ShouldBe(2);
        result.Rows.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Sensitive_Value_Should_Use_Existing_Redaction_Policy()
    {
        var manager = CreateManager(
            out var dataManager, out var repository, out var connection, ValueRetentionModeCodes.Masked);
        dataManager.ResolveAssertionStructureAsync(
                connection, "public", "users", Arg.Any<CancellationToken>())
            .Returns(CreateStructure());
        repository.ReadProjectionRowsAsync(
                Arg.Any<DatabaseConnectionInfo>(),
                Arg.Any<TableDataStructureModel>(),
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<List<string>>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns([Row("Ada@example.com")]);

        var result = await manager.ProjectAsync(connection, CreateRequest());

        result.OutcomeCode.ShouldBe(ProjectionOutcomeCodes.Projected);
        result.Rows.Single()["email"].ShouldBe("A***@***.com");
        result.Rows.Single()["email"].ShouldNotBe("Ada@example.com");
    }

    // islevi: Projection testleri icin katalog, provider, secret ve retention bagimliliklarini birlestirir.
    private static ProjectionManager CreateManager(
        out DatabaseDataComparisonManager dataManager,
        out IProjectionRepository repository,
        out DatabaseConnection connection,
        string retentionMode = ValueRetentionModeCodes.None)
    {
        dataManager = Substitute.For<DatabaseDataComparisonManager>();
        repository = Substitute.For<IProjectionRepository>();
        repository.EngineCode.Returns(DatabaseEngineCodes.PostgreSql);
        var resolver = Substitute.For<IEngineComponentResolver<IProjectionRepository>>();
        resolver.Resolve(DatabaseEngineCodes.PostgreSql).Returns(repository);
        var secretProvider = Substitute.For<ISecretProvider>();
        secretProvider.GetDatabaseCredentialAsync(Arg.Any<string>())
            .Returns(new DatabaseCredentialModel { Username = "reader", Password = "secret" });
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        settings.GetOrNullAsync(DatabaseCheckerSettings.DataComparison.ValueRetentionMode)
            .Returns(retentionMode);
        var manager = new ProjectionManager(
            dataManager,
            new DatabaseConnectionInfoFactory(secretProvider),
            resolver,
            new ValueRetentionPolicyResolver(settings),
            new FindingValueRedactor());
        connection = new DatabaseConnection(Guid.NewGuid())
        {
            Host = "localhost",
            Port = 5432,
            DatabaseName = "test",
            VaultSecretPath = "test/reader",
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };
        return manager;
    }

    // islevi: Unique id anahtari ve email projection kolonu bulunan katalog modeli kurar.
    private static TableDataStructureModel CreateStructure()
        => new()
        {
            SchemaName = "public",
            TableName = "users",
            ColumnNames = ["id", "email"],
            Columns =
            [
                new TableDataColumnModel { Name = "id", CanonicalDataTypeCode = CanonicalDataTypeCodes.Integer },
                new TableDataColumnModel { Name = "email", CanonicalDataTypeCode = CanonicalDataTypeCodes.String }
            ],
            UniqueKeyColumnSets = [["id"]]
        };

    // islevi: Tek unique anahtar ve email projection kolonuyla ortak request kurar.
    private static ProjectionRequest CreateRequest()
        => new()
        {
            SchemaName = "public",
            TableName = "users",
            KeyValues = new Dictionary<string, string?> { ["id"] = "42" },
            ProjectColumns = ["email"]
        };

    // islevi: Tek hassas email degerli ham repository projection satiri kurar.
    private static ProjectionRow Row(string email)
        => new(new Dictionary<string, string?> { ["email"] = email });

}
