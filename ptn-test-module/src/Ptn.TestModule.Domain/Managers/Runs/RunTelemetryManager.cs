using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Kosum span'lerini ve iki OTel semantic convention olcumunu yayar, trace_id koprusunu kurar.
// sistemdeki gorevi: Ayrinti trace'te, hukum veritabaninda kalir; ham log deposu acilmaz (PLAN-0003 TM-16 §2.6).
/// <summary>
/// Kosum icra ve hukum anlarinin telemetrisini yayar.
/// </summary>
public class RunTelemetryManager : TestModuleDomainService
{
    /// <summary>Kosum span'lerini yayan surec genelinde tekil kaynaktir.</summary>
    private static readonly ActivitySource Source = new(RunTelemetryConsts.ActivitySourceName);

    /// <summary>Kosum olcumlerini yayan surec genelinde tekil metre'dir.</summary>
    private static readonly Meter RunMeter = new(RunTelemetryConsts.MeterName);

    /// <summary>Tek bir test durumunun semantic convention sayacidir.</summary>
    private static readonly Counter<long> TestCaseResultStatus =
        RunMeter.CreateCounter<long>(RunTelemetryConsts.Instruments.TestCaseResultStatus);

    /// <summary>Tum kosumun semantic convention sayacidir.</summary>
    private static readonly Counter<long> TestSuiteRunStatus =
        RunMeter.CreateCounter<long>(RunTelemetryConsts.Instruments.TestSuiteRunStatus);

    // Kosum span'ini satirdaki trace kimligine baglayarak baslatir.
    /// <summary>Kosumun icra span'ini kayitli trace baglaminda baslatir.</summary>
    public Activity? StartExecution(TestRun run)
    {
        ArgumentNullException.ThrowIfNull(run);
        return StartSpan(RunTelemetryConsts.Spans.Execute, run);
    }

    // Hukum cozumleme span'ini ayni trace baglaminda baslatir.
    /// <summary>Kosumun hukum span'ini kayitli trace baglaminda baslatir.</summary>
    public Activity? StartJudgement(TestRun run)
    {
        ArgumentNullException.ThrowIfNull(run);
        return StartSpan(RunTelemetryConsts.Spans.Judge, run);
    }

    // Terminal hukmu iki semantic convention sayacina ayni oznitelik kumesiyle yazar.
    /// <summary>Terminal hukmun test durumu ve kosum durumu olcumlerini yayar.</summary>
    public void RecordTerminal(TestRun run, TestRunTerminalModel terminal)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(terminal);
        TestCaseResultStatus.Add(1, CreateCaseTags(run, terminal));
        TestSuiteRunStatus.Add(1, CreateSuiteTags(run, terminal));
    }

    // Kayitli trace kimligi cozulebiliyorsa span'i o trace'in altina, cozulemiyorsa kok olarak acar.
    /// <summary>Verilen adla kosum span'ini baslatir.</summary>
    private static Activity? StartSpan(string name, TestRun run)
    {
        var tags = new ActivityTagsCollection
        {
            [RunTelemetryConsts.Attributes.RunId] = run.Id.ToString("N"),
            [RunTelemetryConsts.Attributes.TestKey] = run.TestKey,
            [RunTelemetryConsts.Attributes.EnvironmentKey] = run.EnvironmentKey,
            [RunTelemetryConsts.Attributes.IsDryRun] = run.IsDryRun
        };

        return TryCreateContext(run.TraceId, out var context)
            ? Source.StartActivity(name, ActivityKind.Internal, context, tags)
            : Source.StartActivity(name, ActivityKind.Internal, parentContext: default, tags);
    }

    // Satirdaki 32 karakterlik W3C trace kimligini gecerli bir span baglamina cevirir.
    /// <summary>Kayitli trace kimliginden aktivite baglami uretir.</summary>
    private static bool TryCreateContext(string? traceId, out ActivityContext context)
    {
        context = default;
        if (string.IsNullOrWhiteSpace(traceId))
        {
            return false;
        }

        try
        {
            context = new ActivityContext(
                ActivityTraceId.CreateFromString(traceId),
                ActivitySpanId.CreateRandom(),
                ActivityTraceFlags.Recorded);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    // Tek test durumunun oznitelik kumesini kurar.
    /// <summary>Test durumu olcumunun oznitelik kumesini uretir.</summary>
    private static TagList CreateCaseTags(TestRun run, TestRunTerminalModel terminal)
    {
        return new TagList
        {
            { RunTelemetryConsts.Attributes.OutcomeCode, terminal.OutcomeCode },
            { RunTelemetryConsts.Attributes.TestKey, run.TestKey },
            { RunTelemetryConsts.Attributes.EnvironmentKey, run.EnvironmentKey },
            { RunTelemetryConsts.Attributes.FailureCategoryCode, terminal.FailureCategoryCode ?? string.Empty },
            { RunTelemetryConsts.Attributes.IsDryRun, run.IsDryRun }
        };
    }

    // Tum kosumun oznitelik kumesini kurar.
    /// <summary>Kosum durumu olcumunun oznitelik kumesini uretir.</summary>
    private static TagList CreateSuiteTags(TestRun run, TestRunTerminalModel terminal)
    {
        return new TagList
        {
            { RunTelemetryConsts.Attributes.RunStatusCode, terminal.RunStatusCode },
            { RunTelemetryConsts.Attributes.TestKey, run.TestKey },
            { RunTelemetryConsts.Attributes.EnvironmentKey, run.EnvironmentKey },
            { RunTelemetryConsts.Attributes.IsDryRun, run.IsDryRun }
        };
    }
}
