using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: Snapshot'tan operasyon ve tek-seviye sema yazarlik ozetleri uretir.
// sistemdeki gorevi: Tam OpenAPI govdesini acmadan verbosity, 2 KB ve resultRef kurallarini tek domain kapisinda uygular.
public sealed class SpecSnapshotAuthoringManager : ITransientDependency
{
    private readonly ISpecSchemaResolver _resolver;
    private readonly OperationResolver _operationResolver;
    private readonly SnapshotAuthoringResultStore _resultStore;

    public SpecSnapshotAuthoringManager(
        ISpecSchemaResolver resolver,
        OperationResolver operationResolver,
        SnapshotAuthoringResultStore resultStore)
    {
        _resolver = resolver;
        _operationResolver = operationResolver;
        _resultStore = resultStore;
    }

    // islevi: Hedef operasyonu cozer, ozetler ve gerekirse tam ozeti resultRef'e koyar.
    public async Task<OperationSummaryResult> FindOperationAsync(
        SpecSnapshot? snapshot,
        OperationSelectionRequest request,
        string verbosityCode)
    {
        if (snapshot?.SpecContent is null)
        {
            return new OperationSummaryResult { OutcomeCode = ConformanceOutcomeCodes.SnapshotNotFound };
        }

        var model = await _resolver.GetSnapshotAsync(snapshot.SpecContent);
        var operation = _operationResolver.Resolve(model, request.OperationId, request.Method, request.Path);
        if (operation is null)
        {
            return new OperationSummaryResult { OutcomeCode = ConformanceOutcomeCodes.OperationNotResolved };
        }

        var full = BuildOperation(operation);
        var output = CloneOperation(full);
        output.TrimToBudget(ResolveFieldLimit(verbosityCode));
        if (output.IsTruncated)
        {
            output.ResultRef = await _resultStore.SaveAsync(new SnapshotAuthoringResultEnvelope
            {
                KindCode = SnapshotAuthoringConstants.OperationResultKind,
                Operation = full
            });
        }

        return output;
    }

    // islevi: Snapshot belgesindeki operasyon envanterini filtreler, sayfalar ve byte tavanina indirger.
    public async Task<SnapshotOperationInventoryResult> ListOperationsAsync(
        SpecSnapshot? snapshot,
        SnapshotOperationInventoryRequest request)
    {
        if (snapshot?.SpecContent is null)
        {
            return new SnapshotOperationInventoryResult
            {
                OutcomeCode = ConformanceOutcomeCodes.SnapshotNotFound
            };
        }

        var model = await _resolver.GetSnapshotAsync(snapshot.SpecContent);
        var matches = FilterOperations(model.Operations, request);
        var result = BuildInventory(matches, request);
        result.TrimToBudget(SnapshotOperationInventoryConsts.MaxResponseBytes);
        return result;
    }

    // islevi: Tek component semasini cozer, bir seviye ozetler ve gerekirse resultRef olusturur.
    public async Task<SchemaDescriptionResult> DescribeSchemaAsync(
        SpecSnapshot? snapshot,
        string schemaRef,
        string verbosityCode)
    {
        if (snapshot?.SpecContent is null)
        {
            return new SchemaDescriptionResult { OutcomeCode = ConformanceOutcomeCodes.SnapshotNotFound };
        }

        var model = await _resolver.GetSnapshotAsync(snapshot.SpecContent);
        var schema = model.Schemas.FirstOrDefault(candidate =>
            candidate.Name == schemaRef || candidate.ReferenceId == schemaRef);
        if (schema is null)
        {
            return new SchemaDescriptionResult { OutcomeCode = ConformanceOutcomeCodes.SchemaNotResolved };
        }

        var full = BuildSchema(schemaRef, schema);
        var output = CloneSchema(full);
        output.TrimToBudget(ResolveFieldLimit(verbosityCode));
        if (output.IsTruncated)
        {
            output.ResultRef = await _resultStore.SaveAsync(new SnapshotAuthoringResultEnvelope
            {
                KindCode = SnapshotAuthoringConstants.SchemaResultKind,
                Schema = full
            });
        }

        return output;
    }

    // islevi: Daha once hesaplanmis tam yazarlik ozetini yeniden calistirmadan bulur.
    public SnapshotAuthoringResultEnvelope? FindResult(string resultRef) => _resultStore.Find(resultRef);

    // islevi: Operasyonun yalniz zorunlu request ve 2xx response yuzeyini ozetler.
    private static OperationSummaryResult BuildOperation(SpecOperationModel operation)
    {
        return new OperationSummaryResult
        {
            OutcomeCode = ConformanceOutcomeCodes.Passed,
            Method = operation.Method,
            Path = operation.Path,
            OperationId = operation.OperationId,
            RequiredParameters = operation.Parameters.Where(parameter => parameter.Required)
                .Select(parameter => new OperationParameterSummary
                    { Name = parameter.Name, Location = parameter.In, Type = parameter.Type }).ToList(),
            RequestMediaTypes = operation.RequestBodies.Select(body => body.MediaType).Distinct().Order().ToList(),
            SuccessStatusCodes = operation.Responses.Where(response => IsSuccess(response.StatusCode))
                .Select(response => response.StatusCode).Distinct().Order().ToList(),
            ResponseFields = BuildResponseFields(operation),
            SecurityRequirements = BuildSecurityRequirements(operation)
        };
    }

    // islevi: Ilk kararli 2xx semasinin alanlarini bir seviye ozetler.
    private static List<SchemaFieldSummary> BuildResponseFields(SpecOperationModel operation)
    {
        var response = operation.Responses.Where(item => IsSuccess(item.StatusCode))
            .OrderBy(item => item.StatusCode, StringComparer.Ordinal)
            .ThenBy(item => item.MediaType, StringComparer.Ordinal)
            .FirstOrDefault(item => item.Schema is not null);
        return response?.Schema is null ? [] : BuildFields(response.Schema.Properties);
    }

    // islevi: Security alternatiflerini sema ve scope adlariyla kararli metinlere indirger.
    private static List<string> BuildSecurityRequirements(SpecOperationModel operation)
    {
        return operation.SecurityRequirements.Select(requirement => string.Join(" & ",
                requirement.Schemes.Select(scheme => $"{scheme.Name}({string.Join(',', scheme.Scopes)})")))
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    // islevi: Envanteri verilen kapali kume filtrelerine daraltir; verilmeyen filtre alani listeyi kisitlamaz.
    private static List<SpecOperationModel> FilterOperations(
        IEnumerable<SpecOperationModel> operations,
        SnapshotOperationInventoryRequest request)
    {
        return operations
            .Where(operation => request.MethodCode is null ||
                                string.Equals(operation.Method, request.MethodCode, StringComparison.OrdinalIgnoreCase))
            .Where(operation => request.PathPrefix is null ||
                                operation.Path.StartsWith(request.PathPrefix, StringComparison.Ordinal))
            .Where(operation => request.HasRequestBody is null ||
                                (operation.RequestBodies.Count > 0) == request.HasRequestBody)
            .ToList();
    }

    // islevi: Eslesen operasyonlari kararli sirada sayfalar ve istenen ile etkin sayfa boyutunu acikca raporlar.
    private static SnapshotOperationInventoryResult BuildInventory(
        List<SpecOperationModel> matches,
        SnapshotOperationInventoryRequest request)
    {
        var effectiveSize = Math.Min(request.MaxResultCount, SnapshotOperationInventoryConsts.MaxPageSize);
        var result = new SnapshotOperationInventoryResult
        {
            OutcomeCode = ConformanceOutcomeCodes.Passed,
            TotalCount = matches.Count,
            RequestedMaxResultCount = request.MaxResultCount,
            EffectiveMaxResultCount = effectiveSize
        };
        result.Items.AddRange(matches
            .OrderBy(operation => operation.Path, StringComparer.Ordinal)
            .ThenBy(operation => operation.Method, StringComparer.Ordinal)
            .Skip(request.SkipCount)
            .Take(effectiveSize)
            .Select(BuildRow));
        return result;
    }

    // islevi: Operasyonu kimlik ve iki sema adresinden ibaret hafif envanter satirina indirger.
    private static SpecOperationRow BuildRow(SpecOperationModel operation)
        => new()
        {
            OperationId = operation.OperationId,
            Method = operation.Method,
            Path = operation.Path,
            RequestSchemaRef = ResolveRequestSchemaRef(operation),
            ResponseSchemaRef = ResolveResponseSchemaRef(operation)
        };

    // islevi: Component'e bagli ilk istek govdesi semasinin referans kimligini secer.
    private static string? ResolveRequestSchemaRef(SpecOperationModel operation)
        => operation.RequestBodies
            .Select(body => body.SchemaReferenceId)
            .FirstOrDefault(reference => reference is not null);

    // islevi: Ilk kararli 2xx yanitin component'e bagli sema referans kimligini secer.
    private static string? ResolveResponseSchemaRef(SpecOperationModel operation)
        => operation.Responses
            .Where(response => IsSuccess(response.StatusCode))
            .OrderBy(response => response.StatusCode, StringComparer.Ordinal)
            .ThenBy(response => response.MediaType, StringComparer.Ordinal)
            .Select(response => response.SchemaReferenceId)
            .FirstOrDefault(reference => reference is not null);

    // islevi: Component semasini enum ve tek-seviye alan listesiyle ozetler.
    private static SchemaDescriptionResult BuildSchema(string schemaRef, SpecSchemaModel schema)
        => new()
        {
            OutcomeCode = ConformanceOutcomeCodes.Passed,
            SchemaRef = schemaRef,
            Type = schema.Type,
            Nullable = schema.Nullable,
            EnumValues = schema.EnumValues.ToList(),
            Fields = BuildFields(schema.Properties)
        };

    // islevi: Property modellerini nested govde tasimayan kararli alan ozetlerine cevirir.
    private static List<SchemaFieldSummary> BuildFields(IEnumerable<SpecSchemaPropertyModel> properties)
        => properties.OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => new SchemaFieldSummary
            {
                Name = property.Name,
                Type = property.Schema?.Type ?? property.Type,
                Required = property.Required,
                Nullable = property.Nullable,
                EnumValues = property.EnumValues.ToList(),
                ReferenceId = property.ReferenceId
            }).ToList();

    // islevi: Operasyon ozetini ilk cevap icin bagimsiz kopyaya cevirir.
    private static OperationSummaryResult CloneOperation(OperationSummaryResult source)
        => new()
        {
            OutcomeCode = source.OutcomeCode,
            Method = source.Method,
            Path = source.Path,
            OperationId = source.OperationId,
            RequiredParameters = source.RequiredParameters.ToList(),
            RequestMediaTypes = source.RequestMediaTypes.ToList(),
            SuccessStatusCodes = source.SuccessStatusCodes.ToList(),
            ResponseFields = source.ResponseFields.ToList(),
            SecurityRequirements = source.SecurityRequirements.ToList()
        };

    // islevi: Sema ozetini ilk cevap icin bagimsiz kopyaya cevirir.
    private static SchemaDescriptionResult CloneSchema(SchemaDescriptionResult source)
        => new()
        {
            OutcomeCode = source.OutcomeCode,
            SchemaRef = source.SchemaRef,
            Type = source.Type,
            Nullable = source.Nullable,
            EnumValues = source.EnumValues.ToList(),
            Fields = source.Fields.ToList()
        };

    // islevi: Verbosity kodunu alan listesi tavanina cevirir.
    private static int ResolveFieldLimit(string verbosityCode)
        => verbosityCode switch
        {
            SnapshotVerbosityCodes.Minimal => SnapshotAuthoringConstants.MinimalFieldCount,
            SnapshotVerbosityCodes.Normal => SnapshotAuthoringConstants.NormalFieldCount,
            SnapshotVerbosityCodes.Full => int.MaxValue,
            _ => throw new ArgumentOutOfRangeException(nameof(verbosityCode))
        };

    // islevi: Tam sayisal 2xx status kodlarini basari olarak tanir.
    private static bool IsSuccess(string statusCode)
        => int.TryParse(statusCode, out var status) && status is >= 200 and <= 299;
}
