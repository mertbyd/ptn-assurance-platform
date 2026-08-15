using System;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Querying;
using NSubstitute;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Interface.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Kosum trace/history kimlikleri ile Pending-Running claim davranisini dogrular.
// sistemdeki gorevi: Tekrar teslimin exception yerine false donmesini ve Guid tabanli trace uretilememesini engeller.
/// <summary>
/// TestRunManager yasam dongusu ve kararli kimlik uretimi testleridir.
/// </summary>
public class TestRunLifecycleTests
{
    // Pending kosumu yalniz ilk teslimde Running durumuna gecirmelidir.
    /// <summary>StartAsync cagrisinin idempotent claim sozlesmesini dogrular.</summary>
    [Fact]
    public async Task Should_claim_pending_run_idempotently()
    {
        var pending = new TestRunStatus(Guid.NewGuid(), "Pending", "Pending");
        var running = new TestRunStatus(Guid.NewGuid(), "Running", "Running");
        var statusRepository = Substitute.For<ITestRunStatusRepository>();
        statusRepository.FindAsync(
                Arg.Any<RepositoryQuery<TestRunStatus>>(),
                Arg.Any<CancellationToken>())
            .Returns(pending, running, pending);
        var manager = new TestRunManager(
            Substitute.For<ITestRunRepository>(),
            statusRepository,
            Substitute.For<ITestTriggerKindRepository>());
        var run = CreateRun(pending.Id);

        var firstClaim = await manager.StartAsync(run, DateTime.UtcNow);
        var secondClaim = await manager.StartAsync(run, DateTime.UtcNow.AddSeconds(1));

        firstClaim.ShouldBeTrue();
        secondClaim.ShouldBeFalse();
        run.RunStatusId.ShouldBe(running.Id);
    }

    // ActivityTraceId uretiminin tam 32 kucuk harfli hex karakter vermesini zorunlu kilar.
    /// <summary>Uretilen trace kimliginin W3C bicimini dogrular.</summary>
    [Fact]
    public void Should_create_lowercase_w3c_trace_id()
    {
        var traceId = TestRunManager.CreateTraceId();

        traceId.Length.ShouldBe(32);
        traceId.ShouldMatch("^[a-f0-9]{32}$");
    }

    // Broken-bar ayiricili kanonik girdinin beklenen SHA-256 degerini uretmesini sabitler.
    /// <summary>History kimliginin kanonik SHA-256 formuluyle ayni oldugunu dogrular.</summary>
    [Fact]
    public void Should_create_canonical_sha256_history_id()
    {
        var historyId = TestRunManager.ComputeHistoryId(
            "checkout",
            "staging",
            "{\"a\":1}");

        historyId.ShouldBe("629a816da7a340487ee5749b1b68e46dad70f95076eac49a86363bec7340046f");
    }

    // Lifecycle testine gereken en kucuk Pending veri kabugunu kurar.
    /// <summary>Verilen durum kimligiyle testte kullanilan TestRun entity'sini olusturur.</summary>
    private static TestRun CreateRun(Guid runStatusId)
    {
        return new TestRun(
            Guid.NewGuid(),
            runStatusId,
            Guid.NewGuid(),
            null,
            new TestRunCreateModel
            {
                TestKey = "checkout",
                TriggerKindCode = "Manual"
            },
            new TestRunEnvironmentBinding
            {
                EnvironmentKey = "staging",
                BaseUrl = "https://staging.example.test",
                SpecSnapshotId = Guid.NewGuid(),
                DbConnectionId = Guid.NewGuid(),
                SecretRef = "vault/staging"
            },
            new string('a', 64),
            new string('b', 32),
            new string('c', 64),
            new string('d', 64),
            "redocly-respect@2.14.0");
    }
}
