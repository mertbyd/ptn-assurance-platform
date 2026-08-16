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

// islevi: Exact capture'in hata ve release-failure yollarinda slot temizleme garantisini dogrular.
// sistemdeki gorevi: Tuketilmeyen slotun WAL biriktirmesine yol acacak finally siz akisi regresyona karsi kapatir.
public class SlotLifecycleTests
{
    [Fact]
    public async Task Capture_Failure_Should_Still_Release_Slot()
    {
        var manager = CreateManager(out var repository, out var connection);
        repository.CaptureAsync(
                Arg.Any<DatabaseConnectionInfo>(), Arg.Any<Guid>(), Arg.Any<List<ComparisonTableIdentifierModel>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WriteSetResult>(
                new InvalidOperationException("capture failed")));
        repository.ReleaseAsync(Arg.Any<DatabaseConnectionInfo>(), Arg.Any<Guid>(), CancellationToken.None)
            .Returns(true);
        var request = CreateRequest();

        var result = await manager.CaptureAsync(connection, request);

        result.StrengthCode.ShouldBe(FootprintStrengthCodes.Unavailable);
        await repository.Received(1).ReleaseAsync(
            Arg.Any<DatabaseConnectionInfo>(), request.CaptureRef, CancellationToken.None);
    }

    [Fact]
    public async Task Release_Failure_Should_Be_Reported_To_Caller()
    {
        var manager = CreateManager(out var repository, out var connection);
        repository.CaptureAsync(
                Arg.Any<DatabaseConnectionInfo>(), Arg.Any<Guid>(), Arg.Any<List<ComparisonTableIdentifierModel>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(new WriteSetResult { StrengthCode = FootprintStrengthCodes.Exact });
        repository.ReleaseAsync(Arg.Any<DatabaseConnectionInfo>(), Arg.Any<Guid>(), CancellationToken.None)
            .Returns(false);

        var result = await manager.CaptureAsync(connection, CreateRequest());

        result.Reasons.ShouldContain(CapabilityReasonCodes.SlotReleaseFailed);
        result.IsAdvisoryOnly.ShouldBeTrue();
    }

    // islevi: Exact probe sonucu veren repository ve comparison fallback resolver'iyle manager kurar.
    private static WriteSetCapabilityManager CreateManager(
        out IWriteSetRepository repository,
        out DatabaseConnection connection)
    {
        repository = Substitute.For<IWriteSetRepository>();
        repository.EngineCode.Returns(DatabaseEngineCodes.PostgreSql);
        repository.ProbeAsync(Arg.Any<DatabaseConnectionInfo>(), Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevel { HasLogicalDecoding = true });
        var writeResolver = Substitute.For<IEngineComponentResolver<IWriteSetRepository>>();
        writeResolver.Resolve(DatabaseEngineCodes.PostgreSql).Returns(repository);
        var dataResolver = Substitute.For<IEngineComponentResolver<IDatabaseDataComparisonRepository>>();
        dataResolver.Resolve(DatabaseEngineCodes.PostgreSql)
            .Returns(Substitute.For<IDatabaseDataComparisonRepository>());
        connection = CreateConnection();
        return new WriteSetCapabilityManager(
            CreateConnectionInfoFactory(), writeResolver, dataResolver, Substitute.For<IDiffWriteSetRepository>());
    }

    // islevi: CaptureRef ve tek schema.table adayi bulunan request kurar.
    private static WriteSetCaptureRequest CreateRequest()
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
