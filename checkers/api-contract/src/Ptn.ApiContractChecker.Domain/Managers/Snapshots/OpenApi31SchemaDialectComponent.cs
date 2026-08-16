using System.Text.Json.Nodes;
using NJsonSchema;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: OpenAPI 3.1 null birlesimi ve JSON Schema dialect kararini uygular.
public class OpenApi31SchemaDialectComponent : SpecSchemaDialectComponentBase, ITransientDependency
{
    public override string FormatCode => SpecFormatCodes.OpenApi31;
    protected override string DialectUri => SpecSchemaDialectUris.Draft202012;
    protected override SchemaType SchemaType => SchemaType.JsonSchema;

    protected override void AddNullable(JsonObject node, string? type)
    {
        var types = string.IsNullOrWhiteSpace(type)
            ? new[] { SpecNormalizationTextConstants.Normalization.NullType }
            : type.Split(SpecNormalizationTextConstants.Normalization.TypeSeparator)
                .Append(SpecNormalizationTextConstants.Normalization.NullType)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        node[SpecSchemaJsonPropertyNames.Type] = new JsonArray(
            types.Select(typeName => JsonValue.Create(typeName)).ToArray<JsonNode?>());
    }
}
