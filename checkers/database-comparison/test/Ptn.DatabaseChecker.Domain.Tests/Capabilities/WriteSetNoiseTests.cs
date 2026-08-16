using NSubstitute;
using Ptn.DatabaseChecker.Constants.Capabilities;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Interface.Capabilities;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Secrets;
using Ptn.DatabaseChecker.Managers.Capabilities;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Capabilities;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Secrets;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Capabilities;

// islevi: Inferred diff sonucunda audit kolonlarinin tablo ayak izini yapay olarak genisletmemesini dogrular.
// sistemdeki gorevi: CreationTime/LastModificationTime/ConcurrencyStamp gurultusunun her operasyonu tum tablo degisimi gostermesini engeller.
public class WriteSetNoiseTests
{
    [Fact]
    public async Task Inferred_Result_Should_Remove_Audit_Only_Table_Changes()
    {
        var manager = CreateManager(out var repository, out var diff, out var connection);
        repository.ProbeAsync(Arg.Any<DatabaseConnectionInfo>(), Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevel { Reasons = [CapabilityReasonCodes.WalLevelNotLogical] });
        diff.CaptureAsync(
                connection, Arg.Any<List<ComparisonTableIdentifierModel>>(), null, Arg.Any<CancellationToken>())
            .Returns(CreateNoisyResult());

        var result = await manager.CaptureAsync(connection, Request());

        result.Tables.ShouldBe(["public.users"]);
        result.Columns.ShouldBeEmpty();
        result.RowDeltas.Select(delta => delta.Table).ShouldBe(["public.users"]);
    }

    // islevi: Bir business kolonlu ve bir audit-only tablo degisimli inferred repository sonucu kurar.
    private static WriteSetResult CreateNoisyResult()
        => new()
        {
            StrengthCode = FootprintStrengthCodes.Inferred,
            Tables = ["public.users", "public.audit_log"],
            Columns =
            [
                "public.users.email",
                "public.users.LastModificationTime",
                "public.audit_log.CreationTime",
                "public.audit_log.ConcurrencyStamp"
            ],
            RowDeltas =
            [
                new WriteSetTableDelta { Table = "public.users", BeforeRowCount = 1, AfterRowCount = 1 },
                new WriteSetTableDelta { Table = "public.audit_log", BeforeRowCount = 4, AfterRowCount = 4 }
            ]
        };

    // islevi: Inferred fallback testine write/diff repository ve comparison resolver'li manager kurar.
    private static WriteSetCapabilityManager CreateManager(
        out IWriteSetRepository repository,
        out IDiffWriteSetRepository diff,
        out DatabaseConnection connection)
    {
        repository = Substitute.For<IWriteSetRepository>();
        repository.EngineCode.Returns(DatabaseEngineCodes.PostgreSql);
        var writeResolver = Substitute.For<IEngineComponentResolver<IWriteSetRepository>>();
        writeResolver.Resolve(DatabaseEngineCodes.PostgreSql).Returns(repository);
        var dataResolver = Substitute.For<IEngineComponentResolver<IDatabaseDataComparisonRepository>>();
        dataResolver.Resolve(DatabaseEngineCodes.PostgreSql)
            .Returns(Substitute.For<IDatabaseDataComparisonRepository>());
        diff = Substitute.For<IDiffWriteSetRepository>();
        connection = CreateConnection();
        return new WriteSetCapabilityManager(
            CreateConnectionInfoFactory(), writeResolver, dataResolver, diff);
    }

    // islevi: Business ve audit-only tablolarini aday gosteren capture request'i kurar.
    private static WriteSetCaptureRequest Request()
        => new()
        {
            CaptureRef = Guid.NewGuid(),
            CandidateTables = ["public.users", "public.audit_log"]
        };

    // islevi: Test manager'ina secret cozumlenebilir runtime baglanti factory'si verir.
    private static DatabaseConnectionInfoFactory CreateConnectionInfoFactory()
    {
        var secrets = Substitute.For<ISecretProvider>();
        secrets.GetDatabaseCredentialAsync(Arg.Any<string>())
            .Returns(new DatabaseCredentialModel { Username = "reader", Password = "secret" });
        return new DatabaseConnectionInfoFactory(secrets);
    }

    // islevi: PostgreSQL engine navigation'i yuklu gorulebilir baglanti kurar.
    private static DatabaseConnection CreateConnection()
        => new(Guid.NewGuid())
        {
            Host = "localhost",
            Port = 5432,
            DatabaseName = "sandbox",
            VaultSecretPath = "test/checker",
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL")
        };
}
