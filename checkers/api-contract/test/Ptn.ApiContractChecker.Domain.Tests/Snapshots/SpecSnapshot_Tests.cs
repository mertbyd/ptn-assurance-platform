using Ptn.ApiContractChecker.Managers.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Snapshots;

// islevi: SpecSnapshot gorulme zamani davranisini SpecIngestionManager uzerinden dogrular.
// sistemdeki gorevi: Dedup tekrarlarinin tarihceyi geriye alarak siralamayi bozmasini engeller.
public class SpecSnapshot_Tests
{
    // Geriye giden son-gorulme zamanini programci hatasi olarak reddeder.
    [Fact]
    public void MarkSeen_Should_Reject_A_Backward_Timestamp()
    {
        var manager = new SpecIngestionManager(null!);
        var now = new DateTime(2026, 8, 7, 9, 0, 0, DateTimeKind.Utc);
        var snapshot = manager.CreateSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            now,
            null);

        Should.Throw<ArgumentOutOfRangeException>(() => manager.MarkSeen(snapshot, now.AddMinutes(-1)));
        snapshot.LastSeenAt.ShouldBe(now);
    }
}
