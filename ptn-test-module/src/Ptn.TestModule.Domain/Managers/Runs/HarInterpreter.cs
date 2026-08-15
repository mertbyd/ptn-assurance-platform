using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Ptn.DatabaseChecker.Constants;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Models.Runs;
using Volo.Abp;
using HarFields = Ptn.TestModule.Constants.Runs.HarArtifactConsts.Fields;

namespace Ptn.TestModule.Managers.Runs;

// islevi: HAR 1.2 govdesini entry listesine cevirir, adim kimligini istek header'i veya echo'dan cozer ve veritabani assertion adimlarini isaretler.
// sistemdeki gorevi: Entry ile senaryo adimi arasindaki bagi konuma degil StepKey esitligine dayandiran tek domain sahibidir (ADR-0021, ADR-0022).
/// <summary>
/// Runner'in urettigi HAR belgesini yargiya hazir hale getirir.
/// </summary>
public class HarInterpreter : TestModuleDomainService
{
    /// <summary>Derlenmis veritabani assertion adimlarini tanitan kararli rota bolumudur.</summary>
    private static readonly string DatabaseAssertionPathSegment =
        $"/{DatabaseCheckerHttpApiConstants.Routes.Assertions}/";

    // Govdeyi cozer, her entry'yi sirala, veritabani adimlarini isaretle ve adim kimligini belgeye gore dogrula.
    /// <summary>HAR icerigini siralanmis, siniflandirilmis ve adima baglanmis belgeye cevirir.</summary>
    public HarDocument Interpret(string harContent, WorkflowDocumentFacts facts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(harContent);
        ArgumentNullException.ThrowIfNull(facts);
        using var document = Parse(harContent);
        var log = GetRequired(document.RootElement, HarFields.Log);
        var entries = ReadEntries(log, new HashSet<string>(facts.StepKeys, StringComparer.Ordinal));

        return new HarDocument
        {
            Entries = entries,
            CreatorName = ReadCreator(log, HarFields.Name),
            CreatorVersion = ReadCreator(log, HarFields.Version),
            HasUnboundEntries = entries.Any(entry => entry.StepKey is null)
        };
    }

    // HAR metnini tek JSON belgesi olarak okur; bozuk govdeyi kararli koda cevirir.
    /// <summary>HAR icerigini JSON belgesine cozer.</summary>
    private static JsonDocument Parse(string harContent)
    {
        try
        {
            return JsonDocument.Parse(harContent);
        }
        catch (JsonException exception)
        {
            throw new BusinessException(
                TestModuleRunErrorCodes.HarNotProduced,
                innerException: exception);
        }
    }

    // Tum entry'leri kaynak sirasini ve ilk entry'ye gore zaman ofsetini koruyarak okur.
    /// <summary>Log altindaki tum entry'leri siralanmis modele cevirir.</summary>
    private static IReadOnlyList<HarEntryModel> ReadEntries(
        JsonElement log,
        IReadOnlySet<string> declaredStepKeys)
    {
        if (!log.TryGetProperty(HarFields.Entries, out var entries) ||
            entries.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var origin = ReadStartedAt(entries.EnumerateArray().FirstOrDefault());
        return entries.EnumerateArray()
            .Select((entry, index) => ReadEntry(entry, index + 1, origin, declaredStepKeys))
            .ToList();
    }

    // Tek bir entry'nin istek, yanit, zamanlama ve korelasyon alanlarini tek geciste modele tasir.
    /// <summary>Tek bir entry'yi sira, veritabani isareti ve dogrulanmis adim kimligiyle kurar.</summary>
    private static HarEntryModel ReadEntry(
        JsonElement entry,
        int ordinal,
        DateTimeOffset? origin,
        IReadOnlySet<string> declaredStepKeys)
    {
        var request = GetOptional(entry, HarFields.Request);
        var response = GetOptional(entry, HarFields.Response);
        var responseBody = ReadBody(response, HarFields.Content);
        var requestStepKey = ReadHeader(request, WorkflowRunnerConsts.StepKeyHeaderName);
        var responseStepKey = ReadStepKey(responseBody);
        var url = ReadString(request, HarFields.Url) ?? string.Empty;

        return new HarEntryModel
        {
            StepKey = ResolveStepKey(requestStepKey, responseStepKey, declaredStepKeys),
            Ordinal = ordinal,
            Method = ReadString(request, HarFields.Method) ?? string.Empty,
            Url = url,
            RequestContentType = ReadMimeType(request, HarFields.PostData),
            RequestBody = ReadBody(request, HarFields.PostData),
            StatusCode = ReadInt(response, HarFields.Status) ?? 0,
            ResponseContentType = ReadMimeType(response, HarFields.Content),
            ResponseBody = responseBody,
            StartedAtMs = ReadOffsetMs(entry, origin),
            TimeMs = ReadInt(entry, HarFields.Time) ?? 0,
            IsDatabaseAssertion = IsDatabaseAssertion(url)
        };
    }

    // HAR request header dizisindeki tek kararli degeri buyuk-kucuk harf duyarsiz okur.
    /// <summary>Istek header'indan adim kimligini okur.</summary>
    private static string? ReadHeader(JsonElement request, string headerName)
    {
        var headers = GetOptional(request, HarFields.Headers);
        if (headers.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var values = headers.EnumerateArray()
            .Where(header => string.Equals(
                ReadString(header, HarFields.Name),
                headerName,
                StringComparison.OrdinalIgnoreCase))
            .Select(header => ReadString(header, HarFields.Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Take(2)
            .ToList();
        return values.Count == 1 ? values[0] : null;
    }

    // Checker yanitindaki korelasyon echo'sunu once nesne, sonra kok alan olarak arar (ADR-0021).
    /// <summary>Yanit govdesine echo edilen adim kimligini okur.</summary>
    private static string? ReadStepKey(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var body = JsonDocument.Parse(responseBody);
            return ReadNestedStepKey(body.RootElement) ??
                   ReadString(body.RootElement, WorkflowRunnerConsts.CorrelationStepKeyField);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Yanit govdesinin herhangi bir seviyesindeki korelasyon nesnesinden adim kimligini okur.
    /// <summary>Ic ice korelasyon nesnesindeki adim kimligini arar.</summary>
    private static string? ReadNestedStepKey(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (element.TryGetProperty(WorkflowRunnerConsts.CorrelationField, out var correlation))
        {
            return ReadString(correlation, WorkflowRunnerConsts.CorrelationStepKeyField);
        }

        return element.EnumerateObject()
            .Select(property => ReadNestedStepKey(property.Value))
            .FirstOrDefault(stepKey => stepKey is not null);
    }

    // Celisen, bildirilmemis veya bos bir kimligi kabul etmez; boylece hukum yanlis adima baglanmaz.
    /// <summary>Header ve echo kaynaklarini belgede bildirilen kumeye gore dogrular.</summary>
    private static string? ResolveStepKey(
        string? requestStepKey,
        string? responseStepKey,
        IReadOnlySet<string> declaredStepKeys)
    {
        if (!string.IsNullOrWhiteSpace(requestStepKey) &&
            !string.IsNullOrWhiteSpace(responseStepKey) &&
            !string.Equals(requestStepKey, responseStepKey, StringComparison.Ordinal))
        {
            return null;
        }

        var stepKey = requestStepKey ?? responseStepKey;
        return !string.IsNullOrWhiteSpace(stepKey) && declaredStepKeys.Contains(stepKey)
            ? stepKey
            : null;
    }

    // Derlenmis veritabani adimlarini rota bolumunden tanir; bu adimlar yeniden cagrilmaz.
    /// <summary>Entry'nin bir Database Checker assertion adimi olup olmadigini bildirir.</summary>
    private static bool IsDatabaseAssertion(string url)
    {
        return url.Contains(DatabaseAssertionPathSegment, StringComparison.OrdinalIgnoreCase);
    }

    // Entry'nin baslangicini ilk entry'ye gore milisaniye ofsetine cevirir.
    /// <summary>Entry'nin kosum baslangicina gore ofsetini hesaplar.</summary>
    private static int? ReadOffsetMs(JsonElement entry, DateTimeOffset? origin)
    {
        var startedAt = ReadStartedAt(entry);
        return startedAt is null || origin is null
            ? null
            : (int)(startedAt.Value - origin.Value).TotalMilliseconds;
    }

    // Entry'nin mutlak baslangic zamanini ISO-8601 degerinden okur.
    /// <summary>Entry'nin mutlak baslangic zamanini getirir.</summary>
    private static DateTimeOffset? ReadStartedAt(JsonElement entry)
    {
        var value = ReadString(entry, HarFields.StartedDateTime);
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed
            : null;
    }

    // Istek veya yanit tarafindaki govde metnini okur.
    /// <summary>Verilen kapsayicidaki govde metnini getirir.</summary>
    private static string? ReadBody(JsonElement element, string containerField)
    {
        return ReadString(GetOptional(element, containerField), HarFields.Text);
    }

    // Istek veya yanit tarafindaki content-type basligini okur.
    /// <summary>Verilen kapsayicidaki content-type degerini getirir.</summary>
    private static string? ReadMimeType(JsonElement element, string containerField)
    {
        return ReadString(GetOptional(element, containerField), HarFields.MimeType);
    }

    // Uretici meta verisinin istenen alanini okur.
    /// <summary>HAR'i ureten aracin istenen meta alanini getirir.</summary>
    private static string ReadCreator(JsonElement log, string field)
    {
        return ReadString(GetOptional(log, HarFields.Creator), field) ?? string.Empty;
    }

    // Zorunlu kok alani okur; yoksa artefakti kullanilamaz sayar.
    /// <summary>Zorunlu kok alani getirir.</summary>
    private static JsonElement GetRequired(JsonElement element, string field)
    {
        return element.TryGetProperty(field, out var value)
            ? value
            : throw new BusinessException(TestModuleRunErrorCodes.HarNotProduced)
                .WithData(nameof(field), field);
    }

    // Opsiyonel nesne alanini okur; yoksa tanimsiz eleman dondurur.
    /// <summary>Opsiyonel nesne alanini getirir.</summary>
    private static JsonElement GetOptional(JsonElement element, string field)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(field, out var value)
            ? value
            : default;
    }

    // Alani metin olarak okur; tur uymazsa null dondurur.
    /// <summary>Verilen alani metin olarak getirir.</summary>
    private static string? ReadString(JsonElement element, string field)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(field, out var value) &&
               value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    // Alani tam sayi olarak okur; tur uymazsa null dondurur.
    /// <summary>Verilen alani tam sayi olarak getirir.</summary>
    private static int? ReadInt(JsonElement element, string field)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(field, out var value) &&
               value.ValueKind == JsonValueKind.Number &&
               value.TryGetInt32(out var parsed)
            ? parsed
            : null;
    }
}
