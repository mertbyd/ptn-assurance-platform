using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Operasyon request semasini alanlara ayirip sinir ve negatif ureticileri butce ve redaksiyonla orkestre eder.
// sistemdeki gorevi: AppService'i sema gezme, ornek secme ve deger saklama kararlarindan uzak tutar.
public sealed class SampleSetManager : ITransientDependency
{
    private readonly ISpecSchemaResolver _schemaResolver;
    private readonly OperationResolver _operationResolver;
    private readonly BoundarySampleGenerator _boundaryGenerator;
    private readonly NegativeSampleGenerator _negativeGenerator;
    private readonly ValueRetentionPolicyResolver _retentionPolicyResolver;
    private readonly FindingValueRedactor _redactor;

    public SampleSetManager(
        ISpecSchemaResolver schemaResolver,
        OperationResolver operationResolver,
        BoundarySampleGenerator boundaryGenerator,
        NegativeSampleGenerator negativeGenerator,
        ValueRetentionPolicyResolver retentionPolicyResolver,
        FindingValueRedactor redactor)
    {
        _schemaResolver = schemaResolver;
        _operationResolver = operationResolver;
        _boundaryGenerator = boundaryGenerator;
        _negativeGenerator = negativeGenerator;
        _retentionPolicyResolver = retentionPolicyResolver;
        _redactor = redactor;
    }

    // Snapshot operasyonundan gerekceli ornekleri uretir, alan tavanini ve retention politikasini uygular.
    public async Task<SampleSetResult> BuildAsync(SpecSnapshot? snapshot, SampleSetRequest request)
    {
        if (snapshot?.SpecContent == null)
        {
            return Empty(ConformanceOutcomeCodes.SnapshotNotFound);
        }

        var model = await _schemaResolver.GetSnapshotAsync(snapshot.SpecContent);
        var operation = _operationResolver.Resolve(model, request.OperationId, request.Method, request.Path);
        if (operation == null)
        {
            return Empty(ConformanceOutcomeCodes.OperationNotResolved);
        }

        var samples = BuildSamples(operation, request);
        var policy = await _retentionPolicyResolver.ResolveAsync();
        return new SampleSetResult(ConformanceOutcomeCodes.Passed, ApplyRetention(samples, policy));
    }

    // Parametre ve ilk kararli request body semasini alan orneklerine cevirir.
    private List<FieldSample> BuildSamples(SpecOperationModel operation, SampleSetRequest request)
    {
        var samples = new List<FieldSample>();
        AddParameterSamples(samples, operation.Parameters, request);
        AddBodySamples(samples, operation.RequestBodies, request);
        return samples;
    }

    // Operasyon parametrelerini konum ve ad sirasiyla alan orneklerine cevirir.
    private void AddParameterSamples(
        ICollection<FieldSample> samples,
        IEnumerable<SpecParameterModel> parameters,
        SampleSetRequest request)
    {
        foreach (var parameter in parameters
                     .OrderBy(item => item.In, StringComparer.Ordinal)
                     .ThenBy(item => item.Name, StringComparer.Ordinal))
        {
            var schema = parameter.Schema ?? BuildParameterSchema(parameter);
            AddFieldSamples(samples, BuildPointer(parameter.In, parameter.Name), schema, parameter.Required, request);
        }
    }

    // Ilk kararli request body semasini kok ve property orneklerine cevirir.
    private void AddBodySamples(
        ICollection<FieldSample> samples,
        IEnumerable<SpecRequestBodyModel> requestBodies,
        SampleSetRequest request)
    {
        var body = requestBodies.OrderBy(item => item.MediaType, StringComparer.Ordinal).FirstOrDefault();
        if (body?.Schema != null)
        {
            AddFieldSamples(samples, BuildPointer(ConformanceAuthoringConstants.BodyPointerSegment),
                body.Schema, body.Required, request);
            AddPropertySamples(samples, BuildPointer(ConformanceAuthoringConstants.BodyPointerSegment),
                body.Schema.Properties, request);
        }
    }

    // Ic ice schema property'lerini JSON Pointer sirasiyla ziyaret eder.
    private void AddPropertySamples(
        ICollection<FieldSample> samples,
        string parentPointer,
        IEnumerable<SpecSchemaPropertyModel> properties,
        SampleSetRequest request)
    {
        foreach (var property in properties.OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            var pointer = string.Concat(parentPointer, BuildPointer(property.Name));
            var schema = property.Schema ?? BuildPropertySchema(property);
            AddFieldSamples(samples, pointer, schema, property.Required, request);
            if (CanDescend(pointer))
            {
                AddPropertySamples(samples, pointer, schema.Properties, request);
            }
        }
    }

    // Ic ice veya dongulu sema ziyaretini mevcut request ornegi derinlik tavaniyla sinirlar.
    private static bool CanDescend(string pointer)
    {
        return pointer.Count(character => character == ConformanceTextConstants.PathSeparator) <=
               ConformanceAuthoringConstants.MaxRequestExampleDepth;
    }

    // Secilen turdeki ureticileri calistirip tek alan tavanini uygular.
    private void AddFieldSamples(
        ICollection<FieldSample> samples,
        string pointer,
        SpecSchemaModel schema,
        bool required,
        SampleSetRequest request)
    {
        var fieldSamples = new List<FieldSample>();
        if (request.SampleKindCode is SampleKindCodes.Boundary or SampleKindCodes.Both)
        {
            fieldSamples.AddRange(_boundaryGenerator.Generate(pointer, schema));
        }

        if (request.SampleKindCode is SampleKindCodes.Negative or SampleKindCodes.Both)
        {
            fieldSamples.AddRange(_negativeGenerator.Generate(pointer, schema, required));
        }

        foreach (var sample in fieldSamples.Take(request.MaxSamplesPerField))
        {
            samples.Add(sample);
        }
    }

    // Parametrenin geriye uyumlu duz alanlarini tam sema modeline cevirir.
    private static SpecSchemaModel BuildParameterSchema(SpecParameterModel parameter)
    {
        return new SpecSchemaModel
        {
            Type = parameter.Type,
            Nullable = parameter.Nullable,
            EnumValues = parameter.EnumValues.ToList()
        };
    }

    // Property'nin geriye uyumlu duz alanlarini tam sema modeline cevirir.
    private static SpecSchemaModel BuildPropertySchema(SpecSchemaPropertyModel property)
    {
        return new SpecSchemaModel
        {
            Type = property.Type,
            Nullable = property.Nullable,
            EnumValues = property.EnumValues.ToList()
        };
    }

    // Tum ornek degerlerini tenant-aware retention politikasindan gecirir.
    private List<FieldSample> ApplyRetention(IEnumerable<FieldSample> samples, ValueRetentionPolicy policy)
    {
        return samples.Select(sample => new FieldSample(
            sample.FieldPointer,
            sample.ConstraintCode,
            sample.SampleKindCode,
            sample.PositionCode,
            _redactor.Redact(sample.Value, policy),
            sample.ExpectedOutcomeCode)).ToList();
    }

    // Pointer segmentlerini RFC 6901 kacisiyla tek adreste birlestirir.
    private static string BuildPointer(params string[] segments)
    {
        return string.Concat(segments.Select(segment => string.Concat(
            ConformanceTextConstants.JsonPointerSeparator,
            segment.Replace(ConformanceTextConstants.JsonPointerTilde,
                    ConformanceTextConstants.JsonPointerEscapedTilde, StringComparison.Ordinal)
                .Replace(ConformanceTextConstants.JsonPointerSeparator,
                    ConformanceTextConstants.JsonPointerEscapedSlash, StringComparison.Ordinal))));
    }

    // Snapshot veya operasyon bulunamadiginda acik outcome ile bos sonuc kurar.
    private static SampleSetResult Empty(string outcomeCode)
    {
        return new SampleSetResult(outcomeCode, []);
    }
}
