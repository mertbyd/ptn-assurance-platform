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
using Ptn.DatabaseChecker.Models.Secrets;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.DatabaseChecker.Capabilities;

// islevi: Write-set capability probe'un baglayici sandbox, motor, logical decoding ve diff fallback sirasini dogrular.
// sistemdeki gorevi: Her dalin ayni contract'i exception siz ve dogru kapali reason koduyla dondurmesini korur.
public class CapabilityProbeTests
{
    [Fact]
    public async Task Shared_Sandbox_Should_Stop_Before_Engine_Resolution()
    {
        var manager = CreateManager(out var writeResolver, out _, out _, out var connection);

        var result = await manager.ProbeAsync(
            connection, new CapabilityProbeRequest { RequiresExclusiveSandbox = false });

        result.StrengthCode.ShouldBe(FootprintStrengthCodes.Unavailable);
        result.Reasons.ShouldBe([CapabilityReasonCodes.SharedEnvironment]);
        writeResolver.DidNotReceiveWithAnyArgs().Resolve(default!);
    }

    [Fact]
    public async Task Unsupported_Engine_Should_Return_Unavailable()
    {
        var manager = CreateManager(out _, out var repository, out _, out var connection);
        repository.ProbeAsync(
                Arg.Any<Ptn.DatabaseChecker.Models.Comparison.DatabaseConnectionInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevel { Reasons = [CapabilityReasonCodes.EngineNotSupported] });

        var result = await manager.ProbeAsync(connection, ExclusiveRequest());

        result.StrengthCode.ShouldBe(FootprintStrengthCodes.Unavailable);
        result.Reasons.ShouldContain(CapabilityReasonCodes.EngineNotSupported);
    }

    [Fact]
    public async Task Logical_Wal_And_Replication_Grant_Should_Return_Exact()
    {
        var manager = CreateManager(out _, out var repository, out _, out var connection);
        repository.ProbeAsync(
                Arg.Any<Ptn.DatabaseChecker.Models.Comparison.DatabaseConnectionInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevel { HasLogicalDecoding = true });

        var result = await manager.ProbeAsync(connection, ExclusiveRequest());

        result.StrengthCode.ShouldBe(FootprintStrengthCodes.Exact);
        result.HasLogicalDecoding.ShouldBeTrue();
        result.Reasons.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(CapabilityReasonCodes.WalLevelNotLogical)]
    [InlineData(CapabilityReasonCodes.NoReplicationGrant)]
    public async Task Missing_Logical_Fact_Should_Fall_Back_To_Inferred_Without_Exception(string reason)
    {
        var manager = CreateManager(out _, out var repository, out _, out var connection);
        repository.ProbeAsync(
                Arg.Any<Ptn.DatabaseChecker.Models.Comparison.DatabaseConnectionInfo>(),
                Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevel { Reasons = [reason] });

        var result = await manager.ProbeAsync(connection, ExclusiveRequest());

        result.StrengthCode.ShouldBe(FootprintStrengthCodes.Inferred);
        result.HasLogicalDecoding.ShouldBeFalse();
        result.Reasons.ShouldContain(reason);
    }

    // islevi: Probe testleri icin secret, write-set resolver, comparison resolver ve PostgreSQL baglantisini kurar.
    private static WriteSetCapabilityManager CreateManager(
        out IEngineComponentResolver<IWriteSetRepository> writeResolver,
        out IWriteSetRepository repository,
        out IDiffWriteSetRepository diffRepository,
        out DatabaseConnection connection)
    {
        repository = Substitute.For<IWriteSetRepository>();
        repository.EngineCode.Returns(DatabaseEngineCodes.PostgreSql);
        writeResolver = Substitute.For<IEngineComponentResolver<IWriteSetRepository>>();
        writeResolver.Resolve(DatabaseEngineCodes.PostgreSql).Returns(repository);
        var dataResolver = Substitute.For<IEngineComponentResolver<IDatabaseDataComparisonRepository>>();
        dataResolver.Resolve(DatabaseEngineCodes.PostgreSql)
            .Returns(Substitute.For<IDatabaseDataComparisonRepository>());
        diffRepository = Substitute.For<IDiffWriteSetRepository>();
        connection = CreateConnection();
        return new WriteSetCapabilityManager(
            CreateConnectionInfoFactory(), writeResolver, dataResolver, diffRepository);
    }

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

    // islevi: Tekil sandbox bildiren ortak probe request'i kurar.
    private static CapabilityProbeRequest ExclusiveRequest()
        => new() { RequiresExclusiveSandbox = true };
}
