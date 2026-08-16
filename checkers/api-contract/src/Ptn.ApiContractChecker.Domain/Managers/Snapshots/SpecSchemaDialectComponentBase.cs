using System.Text.Json.Nodes;
using NJsonSchema;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Interface.Formats;
using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: Provider-bagimsiz sema modelini ortak JSON Schema yuzeyine cevirir.
// sistemdeki gorevi: Dialect siniflarina yalniz nullable ve validator turu kararini birakir.
public abstract class SpecSchemaDialectComponentBase : ISpecSchemaDialectComponent
{
    public abstract string FormatCode { get; }
    protected abstract string DialectUri { get; }
    protected abstract SchemaType SchemaType { get; }

    // NJsonSchema 11.6.1 secildi: runtime JSON validation API'si ve MIT lisansi mevcut ihtiyaci
    // ikinci bir OpenAPI parser'i eklemeden karsilar. Microsoft.OpenApi yalniz dokuman okuyucusudur.
    public async Task<ResolvedSpecSchemaModel> BuildAsync(SpecSchemaModel schema)
    {
        var node = BuildNode(schema);
        node[SpecSchemaJsonPropertyNames.Schema] = DialectUri;
        var schemaNode = await JsonSchema.FromJsonAsync(node.ToJsonString());
        return new ResolvedSpecSchemaModel(schemaNode, SchemaType);
    }

    // Ortak tip, constraint ve alt sema alanlarini JSON dugumunde toplar.
    private JsonObject BuildNode(SpecSchemaModel schema)
    {
        var node = new JsonObject();
        AddType(node, schema);
        AddScalarConstraints(node, schema);
        AddCollectionConstraints(node, schema);
        AddProperties(node, schema);
        AddCompositions(node, schema);
        return node;
    }

    // Kaynak dialect'in nullable temsilini alt sinifa devreder.
    private void AddType(JsonObject node, SpecSchemaModel schema)
    {
        if (!string.IsNullOrWhiteSpace(schema.Type))
        {
            node[SpecSchemaJsonPropertyNames.Type] = schema.Type;
        }

        if (schema.Nullable)
        {
            AddNullable(node, schema.Type);
        }
    }

    protected abstract void AddNullable(JsonObject node, string? type);

    // String, sayi ve enum constraint'lerini kayipsiz tasir.
    private static void AddScalarConstraints(JsonObject node, SpecSchemaModel schema)
    {
        AddValue(node, SpecSchemaJsonPropertyNames.Format, schema.Format);
        AddValue(node, SpecSchemaJsonPropertyNames.Pattern, schema.Pattern);
        AddValue(node, SpecSchemaJsonPropertyNames.MinLength, schema.MinLength);
        AddValue(node, SpecSchemaJsonPropertyNames.MaxLength, schema.MaxLength);
        AddValue(node, SpecSchemaJsonPropertyNames.Minimum, schema.Minimum);
        AddValue(node, SpecSchemaJsonPropertyNames.Maximum, schema.Maximum);
        if (schema.EnumValues.Count > 0)
        {
            node[SpecSchemaJsonPropertyNames.Enum] = new JsonArray(
                schema.EnumValues.Select(value => JsonNode.Parse(value)).ToArray());
        }
    }

    // Dizi ve belgesiz property constraint'lerini tasir.
    private void AddCollectionConstraints(JsonObject node, SpecSchemaModel schema)
    {
        AddValue(node, SpecSchemaJsonPropertyNames.MinItems, schema.MinItems);
        AddValue(node, SpecSchemaJsonPropertyNames.MaxItems, schema.MaxItems);
        if (schema.UniqueItems)
        {
            node[SpecSchemaJsonPropertyNames.UniqueItems] = true;
        }

        node[SpecSchemaJsonPropertyNames.AdditionalProperties] = schema.AdditionalProperties == null
            ? schema.AllowAdditionalProperties
            : BuildNode(schema.AdditionalProperties);
        AddSchema(node, SpecSchemaJsonPropertyNames.Items, schema.Items);
    }

    // Property dugumleri ile required listesini ayni parent semaya ekler.
    private void AddProperties(JsonObject node, SpecSchemaModel schema)
    {
        if (schema.Properties.Count == 0)
        {
            return;
        }

        var properties = new JsonObject();
        foreach (var property in schema.Properties)
        {
            properties[property.Name] = BuildNode(property.Schema ?? BuildPropertySchema(property));
        }

        node[SpecSchemaJsonPropertyNames.Properties] = properties;
        var required = schema.Properties.Where(property => property.Required).Select(property => property.Name);
        node[SpecSchemaJsonPropertyNames.Required] = new JsonArray(
            required.Select(name => JsonValue.Create(name)).ToArray<JsonNode?>());
    }

    // Eski diff alanlarini tam sema dugumu olmayan property'ler icin validator semasina cevirir.
    private static SpecSchemaModel BuildPropertySchema(SpecSchemaPropertyModel property)
    {
        return new SpecSchemaModel
        {
            Type = property.Type,
            Nullable = property.Nullable,
            EnumValues = property.EnumValues
        };
    }

    // anyOf, oneOf ve not semalarini kaynak sirasiyla tasir.
    private void AddCompositions(JsonObject node, SpecSchemaModel schema)
    {
        AddSchemaList(node, SpecSchemaJsonPropertyNames.AnyOf, schema.AnyOf);
        AddSchemaList(node, SpecSchemaJsonPropertyNames.OneOf, schema.OneOf);
        AddSchema(node, SpecSchemaJsonPropertyNames.Not, schema.Not);
    }

    private void AddSchemaList(JsonObject node, string name, IReadOnlyCollection<SpecSchemaModel> schemas)
    {
        if (schemas.Count > 0)
        {
            node[name] = new JsonArray(schemas.Select(schema => BuildNode(schema)).ToArray<JsonNode?>());
        }
    }

    private void AddSchema(JsonObject node, string name, SpecSchemaModel? schema)
    {
        if (schema != null)
        {
            node[name] = BuildNode(schema);
        }
    }

    private static void AddValue<T>(JsonObject node, string name, T? value)
    {
        if (value != null)
        {
            node[name] = JsonValue.Create(value);
        }
    }
}
