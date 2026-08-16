using System.Diagnostics;
using Ptn.DatabaseChecker.Constants;

namespace Ptn.DatabaseChecker;

// islevi: Database Checker'in tek ActivitySource'undan izinli DB kimlik etiketleriyle olcum scope'u baslatir.
// sistemdeki gorevi: Consumer OTel listener'ina assertion, diagnosis ve findings span'larini paket bagimsiz BCL Activity API'siyle yayar.
/// <summary>Database Checker ActivitySource giris noktasi.</summary>
public static class DatabaseCheckerTelemetry
{
    private static readonly ActivitySource Source = new(DatabaseCheckerTelemetryConstants.SourceName);

    /// <summary>Verilen use-case icin opsiyonel motor ve veritabani ad alaniyla Activity baslatir.</summary>
    public static DatabaseCheckerActivityScope StartActivity(
        string activityName,
        string? databaseSystemName = null,
        string? databaseNamespace = null)
    {
        var activity = Source.StartActivity(activityName, ActivityKind.Internal);
        activity?.SetTag(
            DatabaseCheckerTelemetryConstants.Attributes.DatabaseSystemName,
            databaseSystemName);
        activity?.SetTag(
            DatabaseCheckerTelemetryConstants.Attributes.DatabaseNamespace,
            databaseNamespace);
        return new DatabaseCheckerActivityScope(activity);
    }
}
