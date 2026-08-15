using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Kosum span'lerinin trace koprusunu ve iki semantic convention olcumunun yayilmasini dogrular.
// sistemdeki gorevi: Ayrintiyi trace'te, hukmu veritabaninda tutan telemetri sozlesmesini kalici kapiya cevirir (PLAN-0003 TM-16 §2.6).
public class RunTelemetryManagerTests
{
    private const string TraceId = "0123456789abcdef0123456789abcdef";

    // Enstruman adlari OTel semantic convention'daki adlarin aynisi olmalidir.
    [Fact]
    public void Should_use_the_otel_semantic_convention_instrument_names()
    {
        RunTelemetryConsts.Instruments.TestCaseResultStatus.ShouldBe("test.case.result.status");
        RunTelemetryConsts.Instruments.TestSuiteRunStatus.ShouldBe("test.suite.run.status");
    }

    // Kosum span'i satirda duran trace kimligiyle ayni trace'e baglanmalidir.
    [Fact]
    public void Should_bridge_the_span_to_the_stored_trace_id()
    {
        using var listener = CreateActivityListener();

        using var activity = new RunTelemetryManager().StartExecution(CreateRun(TraceId));

        activity.ShouldNotBeNull();
        activity.TraceId.ToString().ShouldBe(TraceId);
    }

    // Trace kimligi yoksa span kok olarak acilmali, telemetri kosumu kirmamalidir.
    [Fact]
    public void Should_start_a_root_span_when_the_run_carries_no_trace_id()
    {
        using var listener = CreateActivityListener();

        using var activity = new RunTelemetryManager().StartExecution(CreateRun(traceId: null));

        activity.ShouldNotBeNull();
        activity.TraceId.ToString().ShouldNotBe(TraceId);
    }

    // Terminal hukum her iki sayaca da bir kez ve dogru durum ozniteligiyle yazilmalidir.
    [Fact]
    public void Should_emit_both_semantic_convention_measurements_for_a_terminal_outcome()
    {
        var measurements = new List<KeyValuePair<string, string>>();
        using var listener = CreateMeterListener(measurements);

        new RunTelemetryManager().RecordTerminal(
            CreateRun(TraceId),
            new TestRunTerminalModel
            {
                OutcomeCode = TestOutcomeStatusCodes.Failed,
                RunStatusCode = TestRunStatusCodes.Completed
            });
        listener.RecordObservableInstruments();

        measurements.ShouldContain(item =>
            item.Key == RunTelemetryConsts.Instruments.TestCaseResultStatus &&
            item.Value == TestOutcomeStatusCodes.Failed);
        measurements.ShouldContain(item =>
            item.Key == RunTelemetryConsts.Instruments.TestSuiteRunStatus &&
            item.Value == TestRunStatusCodes.Completed);
    }

    // Span uretimini testte gorunur kilan dinleyiciyi kurar.
    private static ActivityListener CreateActivityListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == RunTelemetryConsts.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    // Olcumleri ve durum ozniteliklerini toplayan dinleyiciyi kurar.
    private static MeterListener CreateMeterListener(List<KeyValuePair<string, string>> measurements)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == RunTelemetryConsts.MeterName)
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, _, tags, _) =>
        {
            foreach (var tag in tags)
            {
                if (tag.Key == RunTelemetryConsts.Attributes.OutcomeCode ||
                    tag.Key == RunTelemetryConsts.Attributes.RunStatusCode)
                {
                    measurements.Add(new KeyValuePair<string, string>(
                        instrument.Name,
                        tag.Value?.ToString() ?? string.Empty));
                }
            }
        });
        listener.Start();
        return listener;
    }

    // Telemetri testleri icin kararli alanlari olan bir kosum kabugu kurar.
    private static TestRun CreateRun(string? traceId)
    {
        return new TestRun(
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            runStatusId: Guid.NewGuid(),
            triggerKindId: Guid.NewGuid(),
            tenantId: null,
            new TestRunCreateModel { TestKey = "orders.create", TriggerKindCode = TestTriggerKindCodes.Manual },
            new TestRunEnvironmentBinding { EnvironmentKey = "staging", BaseUrl = "https://api.test" },
            historyId: new string('a', 64),
            traceId: traceId!,
            specFingerprint: null,
            dbSchemaFingerprint: null,
            runnerRef: null);
    }
}
