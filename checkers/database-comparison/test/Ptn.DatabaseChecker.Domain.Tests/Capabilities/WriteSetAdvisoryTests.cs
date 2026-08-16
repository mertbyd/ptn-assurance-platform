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

// islevi: Dort footprint strength seviyesinde de IsAdvisoryOnly degismezinin true kalmasini dogrular.
// sistemdeki gorevi: Gozlem sonucunun onaysiz assertion oracle'ina donusmesini tum strateji yollarinda engeller.
public class WriteSetAdvisoryTests
{
    [Theory]
    [InlineData(FootprintStrengthCodes.Exact)]
    [InlineData(FootprintStrengthCodes.RowAddressed)]
    public async Task Exact_Family_Result_Should_Always_Be_Advisory(string strengthCode)
    {
        var manager = CreateManager(out var repository, out _, out var connection);
        repository.ProbeAsync(Arg.Any<DatabaseConnectionInfo>(), Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevel { HasLogicalDecoding = true });
        repository.CaptureAsync(
                Arg.Any<DatabaseConnectionInfo>(), Arg.Any<Guid>(), Arg.Any<List<ComparisonTableIdentifierModel>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(new WriteSetResult { StrengthCode = strengthCode, IsAdvisoryOnly = false });
        repository.ReleaseAsync(Arg.Any<DatabaseConnectionInfo>(), Arg.Any<Guid>(), CancellationToken.None)
            .Returns(true);

        var result = await manager.CaptureAsync(connection, Request());

        result.StrengthCode.ShouldBe(strengthCode);
        result.IsAdvisoryOnly.ShouldBeTrue();
    }

    [Fact]
    public async Task Inferred_Result_Should_Always_Be_Advisory()
    {
        var manager = CreateManager(out var repository, out var diff, out var connection);
        repository.ProbeAsync(Arg.Any<DatabaseConnectionInfo>(), Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevel { Reasons = [CapabilityReasonCodes.WalLevelNotLogical] });
        diff.CaptureAsync(
                connection, Arg.Any<List<ComparisonTableIdentifierModel>>(), null, Arg.Any<CancellationToken>())
            .Returns(new WriteSetResult { StrengthCode = FootprintStrengthCodes.Inferred, IsAdvisoryOnly = false });

        var result = await manager.CaptureAsync(connection, Request());

        result.StrengthCode.ShouldBe(FootprintStrengthCodes.Inferred);
        result.IsAdvisoryOnly.ShouldBeTrue();
    }

    [Fact]
    public async Task Unavailable_Result_Should_Always_Be_Advisory()
    {
        var manager = CreateManager(out _, out _, out var connection);

        var result = await manager.CaptureAsync(null, Request());

        result.StrengthCode.ShouldBe(FootprintStrengthCodes.Unavailable);
        result.IsAdvisoryOnly.ShouldBeTrue();
    }

    // islevi: Advisory testleri icin write/diff repository ve comparison resolver'li manager kurar.
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

    // islevi: Tek adayli ortak capture request'i kurar.
    private static WriteSetCaptureRequest Request()
        => new() { CaptureRef = Guid.NewGuid(), CandidateTables = ["public.users"] };

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
