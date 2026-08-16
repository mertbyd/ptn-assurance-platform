using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: String ve sayi sema sinirlarinin alt, tam ve ust degerlerini mekanik olarak uretir.
// sistemdeki gorevi: Sinir eksenini rastgele veri veya is alani varsayimi olmadan aciklanabilir orneklere cevirir.
public sealed class BoundarySampleGenerator : ITransientDependency
{
    // Tek alan semasindaki tum desteklenen alt ve ust sinir orneklerini kararli sirada uretir.
    public List<FieldSample> Generate(string fieldPointer, SpecSchemaModel schema)
    {
        var samples = new List<FieldSample>();
        AddStringMinimum(samples, fieldPointer, schema);
        AddStringMaximum(samples, fieldPointer, schema);
        AddNumericMinimum(samples, fieldPointer, schema);
        AddNumericMaximum(samples, fieldPointer, schema);
        return samples;
    }

    // minLength icin alt, tam ve ust uzunluklari ekler.
    private static void AddStringMinimum(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (schema.MinLength is not { } minimum)
        {
            return;
        }

        if (minimum > 0)
        {
            samples.Add(BuildStringSample(fieldPointer, ConstraintCodes.MinLength,
                SamplePositionCodes.BelowMin, minimum - 1, schema));
        }

        samples.Add(BuildStringSample(fieldPointer, ConstraintCodes.MinLength,
            SamplePositionCodes.AtMin, minimum, schema));
        samples.Add(BuildStringSample(fieldPointer, ConstraintCodes.MinLength,
            SamplePositionCodes.AboveMin, minimum + 1, schema));
    }

    // maxLength icin alt, tam ve ust uzunluklari ekler.
    private static void AddStringMaximum(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (schema.MaxLength is not { } maximum)
        {
            return;
        }

        if (maximum > 0)
        {
            samples.Add(BuildStringSample(fieldPointer, ConstraintCodes.MaxLength,
                SamplePositionCodes.BelowMax, maximum - 1, schema));
        }

        samples.Add(BuildStringSample(fieldPointer, ConstraintCodes.MaxLength,
            SamplePositionCodes.AtMax, maximum, schema));
        samples.Add(BuildStringSample(fieldPointer, ConstraintCodes.MaxLength,
            SamplePositionCodes.AboveMax, maximum + 1, schema));
    }

    // minimum icin bir birim alt, tam ve bir birim ust sayilari ekler.
    private static void AddNumericMinimum(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (schema.Minimum is not { } minimum)
        {
            return;
        }

        AddNumericSample(samples, fieldPointer, ConstraintCodes.Minimum,
            SamplePositionCodes.BelowMin, minimum - SampleGenerationConsts.NumericBoundaryStep, schema);
        AddNumericSample(samples, fieldPointer, ConstraintCodes.Minimum,
            SamplePositionCodes.AtMin, minimum, schema);
        AddNumericSample(samples, fieldPointer, ConstraintCodes.Minimum,
            SamplePositionCodes.AboveMin, minimum + SampleGenerationConsts.NumericBoundaryStep, schema);
    }

    // maximum icin bir birim alt, tam ve bir birim ust sayilari ekler.
    private static void AddNumericMaximum(
        ICollection<FieldSample> samples,
        string fieldPointer,
        SpecSchemaModel schema)
    {
        if (schema.Maximum is not { } maximum)
        {
            return;
        }

        AddNumericSample(samples, fieldPointer, ConstraintCodes.Maximum,
            SamplePositionCodes.BelowMax, maximum - SampleGenerationConsts.NumericBoundaryStep, schema);
        AddNumericSample(samples, fieldPointer, ConstraintCodes.Maximum,
            SamplePositionCodes.AtMax, maximum, schema);
        AddNumericSample(samples, fieldPointer, ConstraintCodes.Maximum,
            SamplePositionCodes.AboveMax, maximum + SampleGenerationConsts.NumericBoundaryStep, schema);
    }

    // String uzunlugunu JSON degeri ve gercek sema kabul sonucuyla ornege cevirir.
    private static FieldSample BuildStringSample(
        string fieldPointer,
        string constraintCode,
        string positionCode,
        int length,
        SpecSchemaModel schema)
    {
        var value = new string(SampleGenerationConsts.StringSampleCharacter, length);
        return new FieldSample(
            fieldPointer,
            constraintCode,
            SampleKindCodes.Boundary,
            positionCode,
            JsonSerializer.Serialize(value),
            IsStringLengthAccepted(length, schema)
                ? SampleExpectedOutcomeCodes.ShouldAccept
                : SampleExpectedOutcomeCodes.ShouldReject);
    }

    // Sayisal degeri JSON temsili ve gercek sema kabul sonucuyla ornege ekler.
    private static void AddNumericSample(
        ICollection<FieldSample> samples,
        string fieldPointer,
        string constraintCode,
        string positionCode,
        decimal value,
        SpecSchemaModel schema)
    {
        samples.Add(new FieldSample(
            fieldPointer,
            constraintCode,
            SampleKindCodes.Boundary,
            positionCode,
            JsonSerializer.Serialize(value),
            IsNumberAccepted(value, schema)
                ? SampleExpectedOutcomeCodes.ShouldAccept
                : SampleExpectedOutcomeCodes.ShouldReject));
    }

    // Uzunlugun semanin iki siniri arasinda kalip kalmadigini bildirir.
    private static bool IsStringLengthAccepted(int length, SpecSchemaModel schema)
    {
        return (!schema.MinLength.HasValue || length >= schema.MinLength.Value) &&
               (!schema.MaxLength.HasValue || length <= schema.MaxLength.Value);
    }

    // Sayinin semanin iki siniri arasinda kalip kalmadigini bildirir.
    private static bool IsNumberAccepted(decimal value, SpecSchemaModel schema)
    {
        return (!schema.Minimum.HasValue || value >= schema.Minimum.Value) &&
               (!schema.Maximum.HasValue || value <= schema.Maximum.Value);
    }
}
