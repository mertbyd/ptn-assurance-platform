using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Kosum denemelerini CTRF belgesine ceviren saf hesabi ve hukum eslemesini isletir.
// sistemdeki gorevi: Ayni kosumu iki kez ihrac ettiginde bayt-es cikti veren tek CTRF sahibidir; model cagrisi tasimaz (RULE-0005, PLAN-0003 TM-14).
/// <summary>
/// Kosum ihracatinin CTRF belgesini deterministik olarak uretir.
/// </summary>
public class CtrfReportManager : TestModuleDomainService
{
    /// <summary>Bayt-es cikti icin girintisiz sabitlenmis JSON yazici ayaridir.</summary>
    private static readonly JsonWriterOptions WriterOptions = new() { Indented = false };

    // Kosumun tum denemelerini tek CTRF belgesine cevirir.
    /// <summary>Ihracat girdisinden kararli CTRF JSON metnini uretir.</summary>
    public string Create(RunExportSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(source.Run);
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            WriteReport(writer, source);
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    // Ic hukum kodunu CTRF'nin kapali durum degerine kayipsiz esler.
    /// <summary>Terminal hukum kodunun CTRF durum karsiligini getirir.</summary>
    public static string ResolveStatus(string outcomeCode)
    {
        return outcomeCode switch
        {
            TestOutcomeStatusCodes.Passed => CtrfReportConsts.Status.Passed,
            TestOutcomeStatusCodes.Failed => CtrfReportConsts.Status.Failed,
            TestOutcomeStatusCodes.Broken => CtrfReportConsts.Status.Other,
            TestOutcomeStatusCodes.Skipped => CtrfReportConsts.Status.Skipped,
            TestOutcomeStatusCodes.Inconclusive => CtrfReportConsts.Status.Pending,
            _ => throw new BusinessException(TestModuleRunErrorCodes.ArtifactFormatNotSupported)
                .WithData(nameof(outcomeCode), outcomeCode)
        };
    }

    // Belgenin kok sarmalayicisini ve dort bolumunu sabit sirayla yazar.
    /// <summary>CTRF kok nesnesini ve alt bolumlerini yazar.</summary>
    private static void WriteReport(Utf8JsonWriter writer, RunExportSource source)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(CtrfReportConsts.Fields.Results);
        writer.WriteStartObject();
        WriteTool(writer);
        WriteSummary(writer, source);
        WriteTests(writer, source);
        WriteEnvironment(writer, source.Run);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    // Raporu ureten araci kararli adiyla bildirir.
    /// <summary>CTRF tool bolumunu yazar.</summary>
    private static void WriteTool(Utf8JsonWriter writer)
    {
        writer.WritePropertyName(CtrfReportConsts.Fields.Tool);
        writer.WriteStartObject();
        writer.WriteString(CtrfReportConsts.Fields.Name, CtrfReportConsts.ToolName);
        writer.WriteEndObject();
    }

    // Bes hukum sayacini ve kosum zaman penceresini yazar.
    /// <summary>CTRF summary bolumunu yazar.</summary>
    private static void WriteSummary(Utf8JsonWriter writer, RunExportSource source)
    {
        writer.WritePropertyName(CtrfReportConsts.Fields.Summary);
        writer.WriteStartObject();
        writer.WriteNumber(CtrfReportConsts.Fields.Tests, source.Attempts.Count);
        writer.WriteNumber(CtrfReportConsts.Fields.Passed, Count(source, CtrfReportConsts.Status.Passed));
        writer.WriteNumber(CtrfReportConsts.Fields.Failed, Count(source, CtrfReportConsts.Status.Failed));
        writer.WriteNumber(CtrfReportConsts.Fields.Pending, Count(source, CtrfReportConsts.Status.Pending));
        writer.WriteNumber(CtrfReportConsts.Fields.Skipped, Count(source, CtrfReportConsts.Status.Skipped));
        writer.WriteNumber(CtrfReportConsts.Fields.Other, Count(source, CtrfReportConsts.Status.Other));
        writer.WriteNumber(CtrfReportConsts.Fields.Start, ToEpochMilliseconds(source.Run.StartedAt));
        writer.WriteNumber(CtrfReportConsts.Fields.Stop, ToEpochMilliseconds(source.Run.CompletedAt));
        writer.WriteEndObject();
    }

    // Denemeleri deneme numarasi sirasina gore tek tek yazar.
    /// <summary>CTRF tests dizisini yazar.</summary>
    private static void WriteTests(Utf8JsonWriter writer, RunExportSource source)
    {
        writer.WritePropertyName(CtrfReportConsts.Fields.Tests);
        writer.WriteStartArray();
        foreach (var attempt in source.Attempts.OrderBy(item => item.Attempt))
        {
            WriteTest(writer, source.Run, attempt);
        }

        writer.WriteEndArray();
    }

    // Tek denemeyi durumu, suresi ve kaybolmayan ic hukum koduyla yazar.
    /// <summary>Bir CTRF test kaydini yazar.</summary>
    private static void WriteTest(Utf8JsonWriter writer, TestRun run, RunExportAttempt attempt)
    {
        writer.WriteStartObject();
        writer.WriteString(CtrfReportConsts.Fields.Name, CreateTestName(run, attempt));
        writer.WriteString(CtrfReportConsts.Fields.Status, ResolveStatus(attempt.OutcomeCode));
        writer.WriteNumber(CtrfReportConsts.Fields.Duration, attempt.DurationMs);
        writer.WriteString(CtrfReportConsts.Fields.Message, attempt.Detail ?? string.Empty);
        WriteTestExtra(writer, attempt);
        writer.WriteEndObject();
    }

    // Ic hukum kodunu ve adim konumunu extra altinda saklayarak kayipsizligi korur.
    /// <summary>Bir CTRF test kaydinin extra bolumunu yazar.</summary>
    private static void WriteTestExtra(Utf8JsonWriter writer, RunExportAttempt attempt)
    {
        writer.WritePropertyName(CtrfReportConsts.Fields.Extra);
        writer.WriteStartObject();
        writer.WriteString(CtrfReportConsts.Fields.OutcomeCode, attempt.OutcomeCode);
        writer.WriteNumber(CtrfReportConsts.Fields.Attempt, attempt.Attempt);
        writer.WriteString(CtrfReportConsts.Fields.ErrorCode, attempt.ErrorCode ?? string.Empty);
        writer.WriteString(CtrfReportConsts.Fields.FailedStepName, attempt.FailedStepName ?? string.Empty);
        writer.WriteNumber(CtrfReportConsts.Fields.FailedStepOrdinal, attempt.FailedStepOrdinal ?? 0);
        writer.WriteEndObject();
    }

    // Ortam snapshot'ini ve kosum kimlik alanlarini yazar.
    /// <summary>CTRF environment bolumunu yazar.</summary>
    private static void WriteEnvironment(Utf8JsonWriter writer, TestRun run)
    {
        writer.WritePropertyName(CtrfReportConsts.Fields.Environment);
        writer.WriteStartObject();
        writer.WriteString(CtrfReportConsts.Fields.TestEnvironment, run.EnvironmentKey);
        writer.WritePropertyName(CtrfReportConsts.Fields.Extra);
        writer.WriteStartObject();
        writer.WriteString(CtrfReportConsts.Fields.TraceId, run.TraceId ?? string.Empty);
        writer.WriteString(CtrfReportConsts.Fields.RunnerRef, run.RunnerRef ?? string.Empty);
        writer.WriteString(CtrfReportConsts.Fields.SpecFingerprint, run.SpecFingerprint ?? string.Empty);
        writer.WriteString(CtrfReportConsts.Fields.DbSchemaFingerprint, run.DbSchemaFingerprint ?? string.Empty);
        writer.WriteBoolean(CtrfReportConsts.Fields.IsDryRun, run.IsDryRun);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    // Denemeyi test anahtari ve deneme numarasindan kararli bicimde adlandirir.
    /// <summary>Bir denemenin CTRF test adini uretir.</summary>
    private static string CreateTestName(TestRun run, RunExportAttempt attempt)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{run.TestKey}#{attempt.Attempt}");
    }

    // Verilen CTRF durumundaki deneme sayisini hesaplar.
    /// <summary>Belirtilen CTRF durumundaki deneme adedini getirir.</summary>
    private static int Count(RunExportSource source, string status)
    {
        return source.Attempts.Count(attempt => ResolveStatus(attempt.OutcomeCode) == status);
    }

    // Zaman damgasini UTC kabul edip CTRF'nin bekledigi epoch milisaniyesine cevirir.
    /// <summary>Opsiyonel zaman damgasini epoch milisaniyesine cevirir.</summary>
    private static long ToEpochMilliseconds(DateTime? value)
    {
        if (value is null)
        {
            return 0;
        }

        return new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
            .ToUnixTimeMilliseconds();
    }
}
