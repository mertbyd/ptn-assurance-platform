using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Required, tip, uzunluk, sayi, enum, pattern ve format kisitlarini tek tek ihlal eder.
// sistemdeki gorevi: Negatif vakalari rastgele veya is alani degeri uydurmadan sema kuralina baglar.
public sealed class NegativeSampleGenerator : ITransientDependency
{
    // Tek alanin her desteklenen kisit ihlalini kararli sirada uretir.
    public List<FieldSample> Generate(string fieldPointer, SpecSchemaModel schema, bool required)
    {
        var samples = new List<FieldSample>();
        AddRequiredViolation(samples, fieldPointer, required);
        AddTypeViolation(samples, fieldPointer, schema);
        AddLengthViolations(samples, fieldPointer, schema);
        AddNumericViolations(samples, fieldPointer, schema);
        AddEnumViolation(samples, fieldPointer, schema);
        AddPatternViolation(samples, fieldPointer, schema);
        AddFormatViolation(samples, fieldPointer, schema);
        return samples;
    }

    // Zorunlu alan icin deger tasimayan ayri omission ornegi ekler.
    private static void AddRequiredViolation(
        ICollection<FieldSample> samples,
        string fieldPointer,
        bool required)
    {
        if (required)
        {
            samples.Add(BuildRejected(fieldPointer, ConstraintCodes.Required, null));
        }
    }

    // Bildirilen tip kumesinin disinda kalan ilk kararli JSON tipini ekler.
    private static void AddTypeViolation(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (string.IsNullOrWhiteSpace(schema.Type))
        {
            return;
        }

        var value = BuildWrongTypeValue(schema);
        if (value != null)
        {
            samples.Add(BuildRejected(fieldPointer, ConstraintCodes.Type, value));
        }
    }

    // minLength ve maxLength icin aralik disindaki en yakin uzunluklari ekler.
    private static void AddLengthViolations(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (schema.MinLength is > 0)
        {
            samples.Add(BuildRejected(
                fieldPointer,
                ConstraintCodes.MinLength,
                SerializeString(schema.MinLength.Value - 1)));
        }

        if (schema.MaxLength is { } maximum)
        {
            samples.Add(BuildRejected(
                fieldPointer,
                ConstraintCodes.MaxLength,
                SerializeString(maximum + 1)));
        }
    }

    // minimum ve maximum icin aralik disindaki en yakin sayilari ekler.
    private static void AddNumericViolations(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (schema.Minimum is { } minimum)
        {
            samples.Add(BuildRejected(
                fieldPointer,
                ConstraintCodes.Minimum,
                JsonSerializer.Serialize(minimum - SampleGenerationConsts.NumericBoundaryStep)));
        }

        if (schema.Maximum is { } maximum)
        {
            samples.Add(BuildRejected(
                fieldPointer,
                ConstraintCodes.Maximum,
                JsonSerializer.Serialize(maximum + SampleGenerationConsts.NumericBoundaryStep)));
        }
    }

    // Enum'un ilk degerini ayni JSON tipinde katalog disina tasiyan ornegi ekler.
    private static void AddEnumViolation(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        var value = BuildEnumViolation(schema.EnumValues);
        if (value != null)
        {
            samples.Add(BuildRejected(fieldPointer, ConstraintCodes.Enum, value));
        }
    }

    // Pattern metninden turetilen ve regex'e uymadigi kanitlanan ilk stringi ekler.
    private static void AddPatternViolation(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (string.IsNullOrWhiteSpace(schema.Pattern))
        {
            return;
        }

        var value = BuildPatternViolation(schema);
        if (value != null)
        {
            samples.Add(BuildRejected(fieldPointer, ConstraintCodes.Pattern, JsonSerializer.Serialize(value)));
        }
    }

    // Format adindan turetilen acikca bozuk stringi ekler.
    private static void AddFormatViolation(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (!string.IsNullOrWhiteSpace(schema.Format))
        {
            samples.Add(BuildRejected(
                fieldPointer,
                ConstraintCodes.Format,
                JsonSerializer.Serialize(string.Concat(schema.Format, SampleGenerationConsts.InvalidValueSuffix))));
        }
    }

    // Null kabul edilmiyorsa null'u, aksi halde izinli olmayan ilk JSON tipini secer.
    private static string? BuildWrongTypeValue(SpecSchemaModel schema)
    {
        var types = schema.Type!
            .Split(SpecNormalizationTextConstants.Normalization.TypeSeparator)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!schema.Nullable)
        {
            return "null";
        }

        if (!types.Contains(ConformanceAuthoringConstants.BooleanType))
        {
            return "true";
        }

        if (!types.Contains(ConformanceAuthoringConstants.StringType))
        {
            return JsonSerializer.Serialize(SampleGenerationConsts.InvalidValueSuffix);
        }

        if (!types.Contains(ConformanceAuthoringConstants.ArrayType))
        {
            return "[]";
        }

        if (!types.Contains(ConformanceAuthoringConstants.ObjectType))
        {
            return "{}";
        }

        if (!types.Contains(ConformanceAuthoringConstants.NumberType) &&
            !types.Contains(ConformanceAuthoringConstants.IntegerType))
        {
            return "0";
        }

        if (types.Contains(ConformanceAuthoringConstants.IntegerType) &&
            !types.Contains(ConformanceAuthoringConstants.NumberType))
        {
            return "0.5";
        }

        return null;
    }

    // String, sayi, boolean veya null enum icin katalog disi ayni-tip deger uretir.
    private static string? BuildEnumViolation(IReadOnlyCollection<string> values)
    {
        if (values.Count == 0)
        {
            return null;
        }

        var known = values.ToHashSet(StringComparer.Ordinal);
        var first = JsonNode.Parse(values.First());
        if (first is JsonValue scalar && scalar.TryGetValue<string>(out var text))
        {
            return BuildStringEnumViolation(text, known);
        }

        if (first is JsonValue number && number.TryGetValue<decimal>(out var numeric))
        {
            return BuildNumericEnumViolation(numeric, known);
        }

        if (first is JsonValue boolean && boolean.TryGetValue<bool>(out var flag))
        {
            var candidate = JsonSerializer.Serialize(!flag);
            return known.Contains(candidate) ? null : candidate;
        }

        var nullCandidate = JsonSerializer.Serialize(SampleGenerationConsts.InvalidValueSuffix);
        return first == null && !known.Contains(nullCandidate) ? nullCandidate : null;
    }

    // String enum degerini sabit suffix ile katalog disina tasir.
    private static string BuildStringEnumViolation(string value, IReadOnlySet<string> known)
    {
        var candidate = value + SampleGenerationConsts.InvalidValueSuffix;
        while (known.Contains(JsonSerializer.Serialize(candidate)))
        {
            candidate += SampleGenerationConsts.InvalidValueSuffix;
        }

        return JsonSerializer.Serialize(candidate);
    }

    // Sayisal enum degerini birim adimlarla katalog disina tasir.
    private static string BuildNumericEnumViolation(decimal value, IReadOnlySet<string> known)
    {
        var candidate = value + SampleGenerationConsts.NumericBoundaryStep;
        while (known.Contains(JsonSerializer.Serialize(candidate)))
        {
            candidate += SampleGenerationConsts.NumericBoundaryStep;
        }

        return JsonSerializer.Serialize(candidate);
    }

    // Sema uzunluklari icinde kalacak adaylari pattern'e karsi sinar; tum stringleri kabul eden desende ornek uretmez.
    private static string? BuildPatternViolation(SpecSchemaModel schema)
    {
        if (schema.Pattern is not { } pattern)
        {
            return null;
        }

        var minimum = Math.Max(schema.MinLength.GetValueOrDefault(), 1);
        var maximum = Math.Max(schema.MaxLength ?? minimum + SampleGenerationConsts.MaxSamplesPerField, minimum);
        var seed = string.Concat(pattern, SampleGenerationConsts.InvalidValueSuffix);
        try
        {
            for (var index = 0; index < SampleGenerationConsts.MaxSamplesPerField; index++)
            {
                var candidate = string.Concat(new string('!', index), seed);
                candidate = candidate.PadRight(minimum, SampleGenerationConsts.StringSampleCharacter);
                candidate = candidate[..Math.Min(candidate.Length, maximum)];
                if (!Regex.IsMatch(candidate, pattern))
                {
                    return candidate;
                }
            }
        }
        catch (ArgumentException)
        {
            return null;
        }

        return null;
    }

    // Istenen uzunlukta kararli stringi kanonik JSON temsiline cevirir.
    private static string SerializeString(int length)
    {
        return JsonSerializer.Serialize(new string(SampleGenerationConsts.StringSampleCharacter, length));
    }

    // Her negatif degeri ortak gerekce ve ShouldReject sonucu ile kurar.
    private static FieldSample BuildRejected(string fieldPointer, string constraintCode, string? value)
    {
        return new FieldSample(
            fieldPointer,
            constraintCode,
            SampleKindCodes.Negative,
            SamplePositionCodes.Violation,
            value,
            SampleExpectedOutcomeCodes.ShouldReject);
    }
}
