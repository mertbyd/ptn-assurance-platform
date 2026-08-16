using System.Text.Json.Nodes;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Conformance;

// islevi: Operasyonun required request alanlarindan tip uyumlu minimal placeholder iskeleti uretir.
// sistemdeki gorevi: Test yazarinin is degeri uydurmadan gecerli payload kurmasina yardim eder.
public class RequestExampleBuilder : ITransientDependency
{
    private readonly ISpecSchemaResolver _schemaResolver;
    private readonly OperationResolver _operationResolver;

    public RequestExampleBuilder(ISpecSchemaResolver schemaResolver, OperationResolver operationResolver)
    {
        _schemaResolver = schemaResolver;
        _operationResolver = operationResolver;
    }

    public async Task<RequestExampleResult> BuildAsync(
        SpecSnapshot? snapshot,
        OperationSelectionRequest request)
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

        var result = Build(operation);
        result.TrimToBudget();
        return result;
    }

    private static RequestExampleResult Build(SpecOperationModel operation)
    {
        var body = operation.RequestBodies.OrderBy(item => item.MediaType, StringComparer.Ordinal).FirstOrDefault();
        return new RequestExampleResult(
            ConformanceOutcomeCodes.Passed,
            body?.MediaType,
            BuildParameters(operation, ParameterLocationCodes.Path),
            BuildParameters(operation, ParameterLocationCodes.Query),
            BuildParameters(operation, ParameterLocationCodes.Header),
            body?.Schema == null ? null : BuildPlaceholder(body.Schema, 0));
    }

    private static Dictionary<string, JsonNode?> BuildParameters(
        SpecOperationModel operation,
        string location)
    {
        return operation.Parameters
            .Where(parameter => parameter.Required && parameter.In == location)
            .OrderBy(parameter => parameter.Name, StringComparer.Ordinal)
            .ToDictionary(
                parameter => parameter.Name,
                BuildParameterPlaceholder,
                StringComparer.Ordinal);
    }

    private static JsonNode? BuildParameterPlaceholder(SpecParameterModel parameter)
    {
        if (parameter.EnumValues.Count > 0)
        {
            return JsonNode.Parse(parameter.EnumValues[0]);
        }

        return BuildPlaceholder(parameter.Schema ?? new SpecSchemaModel
        {
            Type = parameter.Type,
            Nullable = parameter.Nullable,
            EnumValues = parameter.EnumValues
        }, 0);
    }

    private static JsonNode? BuildPlaceholder(SpecSchemaModel schema, int depth)
    {
        if (schema.EnumValues.Count > 0)
        {
            return JsonNode.Parse(schema.EnumValues[0]);
        }

        var type = ResolveType(schema);
        return type switch
        {
            ConformanceAuthoringConstants.ObjectType => BuildObject(schema, depth),
            ConformanceAuthoringConstants.ArrayType => BuildArray(schema, depth),
            ConformanceAuthoringConstants.IntegerType => JsonValue.Create((long)(schema.Minimum ?? 0)),
            ConformanceAuthoringConstants.NumberType => JsonValue.Create(schema.Minimum ?? 0),
            ConformanceAuthoringConstants.BooleanType => JsonValue.Create(false),
            _ => JsonValue.Create(BuildString(schema))
        };
    }

    private static JsonObject BuildObject(SpecSchemaModel schema, int depth)
    {
        var result = new JsonObject();
        if (depth >= ConformanceAuthoringConstants.MaxRequestExampleDepth)
        {
            return result;
        }

        foreach (var property in schema.Properties.Where(property => property.Required && !property.ReadOnly))
        {
            result[property.Name] = BuildPlaceholder(property.Schema ?? new SpecSchemaModel
            {
                Type = property.Type,
                Nullable = property.Nullable,
                EnumValues = property.EnumValues
            }, depth + 1);
        }

        return result;
    }

    private static JsonArray BuildArray(SpecSchemaModel schema, int depth)
    {
        var result = new JsonArray();
        result.Add(schema.Items == null ? null : BuildPlaceholder(schema.Items, depth + 1));

        return result;
    }

    private static string BuildString(SpecSchemaModel schema)
    {
        var value = schema.Format switch
        {
            ConformanceAuthoringConstants.UuidFormat => ConformanceAuthoringConstants.UuidPlaceholder,
            ConformanceAuthoringConstants.GuidFormat => ConformanceAuthoringConstants.UuidPlaceholder,
            ConformanceAuthoringConstants.DateFormat => ConformanceAuthoringConstants.DatePlaceholder,
            ConformanceAuthoringConstants.DateTimeFormat => ConformanceAuthoringConstants.DateTimePlaceholder,
            _ => ConformanceAuthoringConstants.StringPlaceholder
        };
        return value.PadRight(schema.MinLength.GetValueOrDefault(),
            ConformanceAuthoringConstants.StringPaddingCharacter);
    }

    private static string ResolveType(SpecSchemaModel schema)
    {
        return schema.Type?
                   .Split(SpecNormalizationTextConstants.Normalization.TypeSeparator)
                   .FirstOrDefault(type => type != SpecNormalizationTextConstants.Normalization.NullType)
               ?? (schema.Properties.Count > 0
                   ? ConformanceAuthoringConstants.ObjectType
                   : ConformanceAuthoringConstants.StringType);
    }

    private static RequestExampleResult Empty(string outcome)
    {
        return new RequestExampleResult(
            outcome,
            null,
            new Dictionary<string, JsonNode?>(),
            new Dictionary<string, JsonNode?>(),
            new Dictionary<string, JsonNode?>(),
            null);
    }
}
