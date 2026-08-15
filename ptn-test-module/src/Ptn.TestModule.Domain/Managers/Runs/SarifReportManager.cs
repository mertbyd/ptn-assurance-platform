using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.Models.Runs;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Terminal bulgulari SARIF 2.1.0 sonuclarina ceviren saf hesabi ve severity eslemesini isletir.
// sistemdeki gorevi: Parmak izini checker bulgusunun kalici finding_fingerprint kolonundan okur, yeniden hesaplamaz (PLAN-0003 TM-30).
/// <summary>
/// Kosum bulgularinin SARIF belgesini deterministik olarak uretir.
/// </summary>
public class SarifReportManager : TestModuleDomainService
{
    /// <summary>Bayt-es cikti icin girintisiz sabitlenmis JSON yazici ayaridir.</summary>
    private static readonly JsonWriterOptions WriterOptions = new() { Indented = false };

    // Kosumun tum bulgularini tek SARIF belgesine cevirir.
    /// <summary>Ihracat girdisinden kararli SARIF JSON metnini uretir.</summary>
    public string Create(RunExportSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(source.Run);
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            WriteLog(writer, source);
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    // Ic hukum kodunu SARIF seviyesine esler; yesil ve atlanan hukum sonuc uretmez.
    /// <summary>Terminal hukum kodunun SARIF seviyesini getirir; sonuc uretmeyen hukumde null doner.</summary>
    public static string? ResolveLevel(string outcomeCode)
    {
        return outcomeCode switch
        {
            TestOutcomeStatusCodes.Failed => SarifReportConsts.Level.Error,
            TestOutcomeStatusCodes.Broken => SarifReportConsts.Level.Error,
            TestOutcomeStatusCodes.Inconclusive => SarifReportConsts.Level.Error,
            _ => null
        };
    }

    // Belgenin kok alanlarini ve tek kosum girdisini sabit sirayla yazar.
    /// <summary>SARIF kok nesnesini ve runs dizisini yazar.</summary>
    private static void WriteLog(Utf8JsonWriter writer, RunExportSource source)
    {
        writer.WriteStartObject();
        writer.WriteString(SarifReportConsts.Fields.Schema, SarifReportConsts.SchemaUri);
        writer.WriteString(SarifReportConsts.Fields.Version, SarifReportConsts.Version);
        writer.WritePropertyName(SarifReportConsts.Fields.Runs);
        writer.WriteStartArray();
        writer.WriteStartObject();
        WriteTool(writer, source);
        WriteResults(writer, source);
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    // Araci ve bulgularda gecen kural kimliklerini kararli sirayla bildirir.
    /// <summary>SARIF tool bolumunu kural listesiyle yazar.</summary>
    private static void WriteTool(Utf8JsonWriter writer, RunExportSource source)
    {
        writer.WritePropertyName(SarifReportConsts.Fields.Tool);
        writer.WriteStartObject();
        writer.WritePropertyName(SarifReportConsts.Fields.Driver);
        writer.WriteStartObject();
        writer.WriteString(SarifReportConsts.Fields.Name, SarifReportConsts.ToolName);
        writer.WritePropertyName(SarifReportConsts.Fields.Rules);
        writer.WriteStartArray();
        foreach (var ruleId in ReadRuleIds(source))
        {
            writer.WriteStartObject();
            writer.WriteString(SarifReportConsts.Fields.Id, ruleId);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    // Sonuc ureten her denemenin bulgularini deneme ve ordinal sirasina gore yazar.
    /// <summary>SARIF results dizisini yazar.</summary>
    private static void WriteResults(Utf8JsonWriter writer, RunExportSource source)
    {
        writer.WritePropertyName(SarifReportConsts.Fields.Results);
        writer.WriteStartArray();
        foreach (var attempt in source.Attempts.OrderBy(item => item.Attempt))
        {
            WriteAttemptResults(writer, attempt);
        }

        writer.WriteEndArray();
    }

    // Tek denemenin seviyesini cozup bulgularini tek tek yazar.
    /// <summary>Bir denemenin tum bulgu sonuclarini yazar.</summary>
    private static void WriteAttemptResults(Utf8JsonWriter writer, RunExportAttempt attempt)
    {
        var level = ResolveLevel(attempt.OutcomeCode);
        if (level is null)
        {
            return;
        }

        foreach (var finding in attempt.Findings.OrderBy(item => item.Ordinal))
        {
            WriteResult(writer, attempt, finding, level);
        }
    }

    // Tek bulguyu kural kimligi, seviye, konum ve kalici parmak iziyle yazar.
    /// <summary>Bir SARIF sonucunu yazar.</summary>
    private static void WriteResult(
        Utf8JsonWriter writer,
        RunExportAttempt attempt,
        TestResultFinding finding,
        string level)
    {
        writer.WriteStartObject();
        writer.WriteString(SarifReportConsts.Fields.RuleId, ReadRuleId(finding));
        writer.WriteString(SarifReportConsts.Fields.Level, level);
        writer.WritePropertyName(SarifReportConsts.Fields.Message);
        writer.WriteStartObject();
        writer.WriteString(SarifReportConsts.Fields.Text, finding.Message);
        writer.WriteEndObject();
        WriteLocation(writer, finding);
        WriteFingerprints(writer, finding);
        WriteProperties(writer, attempt, finding);
        writer.WriteEndObject();
    }

    // Bulgunun makine-okur konumunu SARIF artifact adresine cevirir.
    /// <summary>Bir SARIF sonucunun konum bolumunu yazar.</summary>
    private static void WriteLocation(Utf8JsonWriter writer, TestResultFinding finding)
    {
        writer.WritePropertyName(SarifReportConsts.Fields.Locations);
        writer.WriteStartArray();
        writer.WriteStartObject();
        writer.WritePropertyName(SarifReportConsts.Fields.PhysicalLocation);
        writer.WriteStartObject();
        writer.WritePropertyName(SarifReportConsts.Fields.ArtifactLocation);
        writer.WriteStartObject();
        writer.WriteString(SarifReportConsts.Fields.Uri, finding.Location);
        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.WriteEndArray();
    }

    // Kalici parmak izini oldugu gibi yazar; ihracat aninda yeniden hesaplanmaz.
    /// <summary>Bir SARIF sonucunun partialFingerprints bolumunu yazar.</summary>
    private static void WriteFingerprints(Utf8JsonWriter writer, TestResultFinding finding)
    {
        writer.WritePropertyName(SarifReportConsts.Fields.PartialFingerprints);
        writer.WriteStartObject();
        writer.WriteString(SarifReportConsts.FingerprintKey, finding.Fingerprint);
        writer.WriteEndObject();
    }

    // Ic hukum kodunu ve bulgu ayrintisini properties altinda saklayarak kayipsizligi korur.
    /// <summary>Bir SARIF sonucunun properties bolumunu yazar.</summary>
    private static void WriteProperties(
        Utf8JsonWriter writer,
        RunExportAttempt attempt,
        TestResultFinding finding)
    {
        writer.WritePropertyName(SarifReportConsts.Fields.Properties);
        writer.WriteStartObject();
        writer.WriteString(SarifReportConsts.Fields.OutcomeCode, attempt.OutcomeCode);
        writer.WriteNumber(SarifReportConsts.Fields.Attempt, attempt.Attempt);
        writer.WriteString(SarifReportConsts.Fields.SourceCheckerCode, finding.SourceCheckerCode);
        writer.WriteString(SarifReportConsts.Fields.ComparisonKindCode, finding.ComparisonKindCode);
        writer.WriteString(SarifReportConsts.Fields.RuleRef, finding.RuleRef ?? string.Empty);
        writer.WriteString(SarifReportConsts.Fields.ExpectedValue, finding.ExpectedValue ?? string.Empty);
        writer.WriteString(SarifReportConsts.Fields.ObservedValue, finding.ObservedValue ?? string.Empty);
        writer.WriteEndObject();
    }

    // Bulgunun kural kimligini kural referansindan, yoksa karsilastirma turunden cozer.
    /// <summary>Bir bulgunun SARIF kural kimligini getirir.</summary>
    private static string ReadRuleId(TestResultFinding finding)
    {
        return string.IsNullOrWhiteSpace(finding.RuleRef)
            ? finding.ComparisonKindCode
            : finding.RuleRef;
    }

    // Sonuc ureten bulgularin kural kimliklerini tekrarsiz ve kararli sirada toplar.
    /// <summary>Belgede bildirilecek kural kimliklerini getirir.</summary>
    private static IReadOnlyList<string> ReadRuleIds(RunExportSource source)
    {
        return source.Attempts
            .Where(attempt => ResolveLevel(attempt.OutcomeCode) is not null)
            .SelectMany(attempt => attempt.Findings)
            .Select(ReadRuleId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(ruleId => ruleId, StringComparer.Ordinal)
            .ToList();
    }
}
