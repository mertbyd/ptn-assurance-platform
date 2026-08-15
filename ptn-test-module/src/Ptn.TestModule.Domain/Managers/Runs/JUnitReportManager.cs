using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Kosum denemelerini JUnit XML belgesine ceviren saf hesabi ve hukum eslemesini isletir.
// sistemdeki gorevi: Failed'i <failure>, Broken'i <error> olarak ayri tutan tek JUnit sahibidir; ayrim duzlestirilmez (PLAN-0003 TM-14 §2.2).
/// <summary>
/// Kosum ihracatinin JUnit XML belgesini deterministik olarak uretir.
/// </summary>
public class JUnitReportManager : TestModuleDomainService
{
    /// <summary>Saniye cinsinden sure niteliginin kararli bicimidir.</summary>
    private const string TimeFormat = "0.000";

    /// <summary>Bayt-es cikti icin girintisiz ve bildirimsiz sabitlenmis XML yazici ayaridir.</summary>
    private static readonly XmlWriterSettings WriterSettings = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.Replace
    };

    // Kosumun tum denemelerini tek JUnit belgesine cevirir.
    /// <summary>Ihracat girdisinden kararli JUnit XML metnini uretir.</summary>
    public string Create(RunExportSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(source.Run);
        var builder = new StringBuilder(JUnitReportConsts.XmlDeclaration);
        using (var writer = XmlWriter.Create(builder, WriterSettings))
        {
            WriteSuites(writer, source);
        }

        return builder.ToString();
    }

    // Ic hukum kodunu JUnit'in cocuk elementine kayipsiz esler; Passed cocuk tasimaz.
    /// <summary>Terminal hukum kodunun JUnit cocuk element adini getirir; Passed icin null doner.</summary>
    public static string? ResolveChildElement(string outcomeCode)
    {
        return outcomeCode switch
        {
            TestOutcomeStatusCodes.Passed => null,
            TestOutcomeStatusCodes.Failed => JUnitReportConsts.Elements.Failure,
            TestOutcomeStatusCodes.Broken => JUnitReportConsts.Elements.Error,
            TestOutcomeStatusCodes.Skipped => JUnitReportConsts.Elements.Skipped,
            TestOutcomeStatusCodes.Inconclusive => JUnitReportConsts.Elements.Error,
            _ => throw new BusinessException(TestModuleRunErrorCodes.ArtifactFormatNotSupported)
                .WithData(nameof(outcomeCode), outcomeCode)
        };
    }

    // Kok testsuites elementini toplam sayaclariyla yazar.
    /// <summary>JUnit kok elementini ve tek suite'i yazar.</summary>
    private static void WriteSuites(XmlWriter writer, RunExportSource source)
    {
        writer.WriteStartElement(JUnitReportConsts.Elements.TestSuites);
        WriteCounters(writer, source);
        WriteSuite(writer, source);
        writer.WriteEndElement();
    }

    // Kosumu tek suite olarak yazip denemelerini icine yerlestirir.
    /// <summary>Kosumun JUnit suite elementini yazar.</summary>
    private static void WriteSuite(XmlWriter writer, RunExportSource source)
    {
        writer.WriteStartElement(JUnitReportConsts.Elements.TestSuite);
        writer.WriteAttributeString(JUnitReportConsts.Attributes.Name, source.Run.TestKey);
        WriteCounters(writer, source);
        writer.WriteAttributeString(JUnitReportConsts.Attributes.Hostname, source.Run.EnvironmentKey);
        foreach (var attempt in source.Attempts.OrderBy(item => item.Attempt))
        {
            WriteCase(writer, source.Run, attempt);
        }

        writer.WriteEndElement();
    }

    // Toplam, basarisizlik, hata ve atlama sayaclarini ayni sirayla yazar.
    /// <summary>Suite ve kok elementin ortak sayac niteliklerini yazar.</summary>
    private static void WriteCounters(XmlWriter writer, RunExportSource source)
    {
        writer.WriteAttributeString(
            JUnitReportConsts.Attributes.Tests,
            source.Attempts.Count.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(
            JUnitReportConsts.Attributes.Failures,
            CountChild(source, JUnitReportConsts.Elements.Failure).ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(
            JUnitReportConsts.Attributes.Errors,
            CountChild(source, JUnitReportConsts.Elements.Error).ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(
            JUnitReportConsts.Attributes.Skipped,
            CountChild(source, JUnitReportConsts.Elements.Skipped).ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(JUnitReportConsts.Attributes.Time, FormatSeconds(TotalDurationMs(source)));
    }

    // Tek denemeyi testcase olarak yazip hukum karsiligi cocugunu ekler.
    /// <summary>Bir denemenin JUnit testcase elementini yazar.</summary>
    private static void WriteCase(XmlWriter writer, TestRun run, RunExportAttempt attempt)
    {
        writer.WriteStartElement(JUnitReportConsts.Elements.TestCase);
        writer.WriteAttributeString(
            JUnitReportConsts.Attributes.Name,
            string.Create(CultureInfo.InvariantCulture, $"{run.TestKey}#{attempt.Attempt}"));
        writer.WriteAttributeString(JUnitReportConsts.Attributes.ClassName, run.EnvironmentKey);
        writer.WriteAttributeString(JUnitReportConsts.Attributes.Time, FormatSeconds(attempt.DurationMs));
        WriteOutcomeChild(writer, attempt);
        writer.WriteEndElement();
    }

    // Passed disindaki her hukum icin kendi elementini mesaj ve tip nitelikleriyle yazar.
    /// <summary>Denemenin hukum karsiligi JUnit cocuk elementini yazar.</summary>
    private static void WriteOutcomeChild(XmlWriter writer, RunExportAttempt attempt)
    {
        var element = ResolveChildElement(attempt.OutcomeCode);
        if (element is null)
        {
            return;
        }

        writer.WriteStartElement(element);
        writer.WriteAttributeString(JUnitReportConsts.Attributes.Message, attempt.Detail ?? string.Empty);
        writer.WriteAttributeString(JUnitReportConsts.Attributes.Type, attempt.ErrorCode ?? attempt.OutcomeCode);
        writer.WriteEndElement();
    }

    // Verilen cocuk elemente dusen deneme sayisini hesaplar.
    /// <summary>Belirtilen JUnit cocuk elementine dusen deneme adedini getirir.</summary>
    private static int CountChild(RunExportSource source, string element)
    {
        return source.Attempts.Count(attempt => ResolveChildElement(attempt.OutcomeCode) == element);
    }

    // Suite suresini denemelerin toplamindan hesaplar.
    /// <summary>Kosumun toplam deneme suresini milisaniye olarak getirir.</summary>
    private static long TotalDurationMs(RunExportSource source)
    {
        return source.Attempts.Sum(attempt => (long)attempt.DurationMs);
    }

    // Milisaniyeyi JUnit'in bekledigi kultur bagimsiz saniye bicimine cevirir.
    /// <summary>Milisaniye suresini kararli saniye metnine cevirir.</summary>
    private static string FormatSeconds(long durationMs)
    {
        return (durationMs / 1000d).ToString(TimeFormat, CultureInfo.InvariantCulture);
    }
}
