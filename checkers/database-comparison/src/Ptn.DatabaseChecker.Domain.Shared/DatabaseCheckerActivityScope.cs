using System.Diagnostics;
using Ptn.DatabaseChecker.Constants;

namespace Ptn.DatabaseChecker;

// islevi: Tek Activity'nin izinli etiketlerini ve sure olcumunu yasam dongusu sonunda tamamlar.
// sistemdeki gorevi: Span ureten manager/repository'lerin Stopwatch ve attribute stringlerini tekrar etmesini engeller.
/// <summary>Database Checker span'inin izinli attribute ve sure yasam dongusu.</summary>
public sealed class DatabaseCheckerActivityScope : IDisposable
{
    private readonly Activity? _activity;
    private readonly long _startedTimestamp = Stopwatch.GetTimestamp();

    // islevi: Opsiyonel Activity'yi sure olcum scope'una baglar.
    internal DatabaseCheckerActivityScope(Activity? activity)
    {
        _activity = activity;
    }

    /// <summary>Span'a kararli is sonucu kodunu ekler.</summary>
    public void SetOutcomeCode(string outcomeCode)
        => _activity?.SetTag(DatabaseCheckerTelemetryConstants.Attributes.OutcomeCode, outcomeCode);

    /// <summary>Span'a assertion deneme sayisini ekler.</summary>
    public void SetAttemptCount(int attemptCount)
        => _activity?.SetTag(DatabaseCheckerTelemetryConstants.Attributes.AttemptCount, attemptCount);

    /// <summary>Span'a diagnosis probe sayisini ekler.</summary>
    public void SetProbeCount(int probeCount)
        => _activity?.SetTag(DatabaseCheckerTelemetryConstants.Attributes.ProbeCount, probeCount);

    /// <summary>Sure attribute'unu yazar ve Activity'yi tamamlar.</summary>
    public void Dispose()
    {
        _activity?.SetTag(
            DatabaseCheckerTelemetryConstants.Attributes.DurationMilliseconds,
            Stopwatch.GetElapsedTime(_startedTimestamp).TotalMilliseconds);
        _activity?.Dispose();
    }
}
