using System.Text.RegularExpressions;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Snapshots;

// islevi: Spec snapshot'ini surum, sira, referans, allOf, path parametresi ve whitespace gurultusundan arindirir.
// sistemdeki gorevi: Okuyucu ile diff motoru arasindaki tek saf normalizasyon noktasidir; provider, I/O ve repository bilgisi tasimaz.
public partial class SpecSnapshotNormalizer : ITransientDependency
{
    // Path parametresi adlarini endpoint kimliginden cikaran ortak regex.
    [GeneratedRegex(
        SpecNormalizationTextConstants.Normalization.PathParameterPattern,
        RegexOptions.Compiled)]
    private static partial Regex PathParameterRegex();

    // Dokumantasyon metinlerindeki satir sonu ve ardisik bosluklari tek bosluga indiren ortak regex.
    [GeneratedRegex(
        SpecNormalizationTextConstants.Normalization.WhitespacePattern,
        RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    // OAS 3.1 coklu tip ifadesi ile kutuphane flag metnini ayni parcalara ayiran ortak regex.
    [GeneratedRegex(
        SpecNormalizationTextConstants.Normalization.TypeSeparatorPattern,
        RegexOptions.Compiled)]
    private static partial Regex TypeSeparatorRegex();

    // islevi: Snapshot'in tum yapisal ve dokumantasyon alanlarini yeni bir deterministik modele indirger.
    public SpecSnapshotModel Normalize(SpecSnapshotModel snapshot)
    {
        var schemasByName = IndexSchemas(snapshot.Schemas);

        return new SpecSnapshotModel
        {
            ApiVersion = NormalizeOptionalText(snapshot.ApiVersion),
            Servers = snapshot.Servers
                .Select(server => server.Trim())
                .Where(server => server.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(server => server, StringComparer.Ordinal)
                .ToList(),
            Operations = snapshot.Operations
                .Select(operation => NormalizeOperation(operation, schemasByName))
                .OrderBy(operation => operation.Path, StringComparer.Ordinal)
                .ThenBy(operation => operation.Method, StringComparer.Ordinal)
                .ThenBy(operation => operation.OperationId, StringComparer.Ordinal)
                .ToList(),
            Schemas = snapshot.Schemas
                .Select(schema => NormalizeSchema(schema, schemasByName, new HashSet<string>(StringComparer.Ordinal)))
                .OrderBy(schema => schema.Name, StringComparer.Ordinal)
                .ToList(),
            Documentation = snapshot.Documentation
                .Select(NormalizeDocumentation)
                .OrderBy(documentation => documentation.TargetKind, StringComparer.Ordinal)
                .ThenBy(documentation => documentation.Target, StringComparer.Ordinal)
                .ThenBy(documentation => documentation.Summary, StringComparer.Ordinal)
                .ThenBy(documentation => documentation.Description, StringComparer.Ordinal)
                .ThenBy(documentation => documentation.Example, StringComparer.Ordinal)
                .ToList()
        };
    }

    // islevi: Sema referanslarini tek geciste cozebilmek icin component adiyla indeksler.
    private static Dictionary<string, SpecSchemaModel> IndexSchemas(IEnumerable<SpecSchemaModel> schemas)
    {
        var schemasByName = new Dictionary<string, SpecSchemaModel>(StringComparer.Ordinal);
        foreach (var schema in schemas)
        {
            schemasByName[schema.Name] = schema;
        }

        return schemasByName;
    }

    // islevi: Operasyon kimligini, listelerini ve alt sozlesmelerini kararli bicime getirir.
    private static SpecOperationModel NormalizeOperation(
        SpecOperationModel operation,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName)
    {
        return new SpecOperationModel
        {
            Path = NormalizePath(operation.Path),
            Method = operation.Method.Trim().ToUpperInvariant(),
            OperationId = NormalizeOptionalText(operation.OperationId),
            IsInternal = operation.IsInternal,
            Tags = operation.Tags
                .Select(tag => tag.Trim())
                .OrderBy(tag => tag, StringComparer.Ordinal)
                .ToList(),
            SecurityRequirements = operation.SecurityRequirements
                .Select(NormalizeSecurityRequirement)
                .OrderBy(BuildSecurityRequirementKey, StringComparer.Ordinal)
                .ToList(),
            Parameters = operation.Parameters
                .Select(parameter => NormalizeParameter(parameter, schemasByName))
                .OrderBy(parameter => parameter.In, StringComparer.Ordinal)
                .ThenBy(parameter => parameter.Name, StringComparer.Ordinal)
                .ToList(),
            RequestBodies = operation.RequestBodies
                .Select(body => NormalizeRequestBody(body, schemasByName))
                .OrderBy(body => body.MediaType, StringComparer.Ordinal)
                .ToList(),
            Responses = operation.Responses
                .Select(response => NormalizeResponse(response, schemasByName))
                .OrderBy(response => response.StatusCode, StringComparer.Ordinal)
                .ThenBy(response => response.MediaType, StringComparer.Ordinal)
                .ToList()
        };
    }

    // islevi: Path sablonundaki parametre adlarini tek maskeye cevirir.
    private static string NormalizePath(string path)
    {
        return PathParameterRegex().Replace(
            path.Trim(),
            SpecNormalizationTextConstants.Normalization.PathParameterMask);
    }

    // islevi: Security sema ve scope listelerini siradan bagimsiz hale getirir.
    private static SpecSecurityRequirementModel NormalizeSecurityRequirement(
        SpecSecurityRequirementModel requirement)
    {
        return new SpecSecurityRequirementModel
        {
            Schemes = requirement.Schemes
                .Select(scheme => new SpecSecuritySchemeModel
                {
                    Name = scheme.Name.Trim(),
                    Scopes = scheme.Scopes
                        .Select(scope => scope.Trim())
                        .OrderBy(scope => scope, StringComparer.Ordinal)
                        .ToList()
                })
                .OrderBy(scheme => scheme.Name, StringComparer.Ordinal)
                .ToList()
        };
    }

    // islevi: Security requirement alternatiflerini siralamak icin sema ve scope'lardan kararli anahtar uretir.
    private static string BuildSecurityRequirementKey(SpecSecurityRequirementModel requirement)
    {
        return string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            requirement.Schemes.Select(scheme => string.Join(
                SpecNormalizationTextConstants.Normalization.TypeSeparator,
                new[] { scheme.Name }.Concat(scheme.Scopes))));
    }

    // islevi: Parametre kimligi ile tip/null sozlesmesini kararli bicime getirir.
    private static SpecParameterModel NormalizeParameter(
        SpecParameterModel parameter,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName)
    {
        var normalizedType = NormalizeType(parameter.Type, parameter.Nullable);
        var enumValues = NormalizeEnumValues(parameter.EnumValues);
        var resolvingReferences = new HashSet<string>(StringComparer.Ordinal);
        if (TryResolveSchema(parameter.ReferenceId, schemasByName, resolvingReferences, out var referencedSchema))
        {
            var normalizedReference = NormalizeSchema(referencedSchema!, schemasByName, resolvingReferences);
            normalizedType = PreferOwnType(normalizedType, normalizedReference);
            enumValues = PreferOwnEnum(enumValues, normalizedReference.EnumValues);
        }

        return new SpecParameterModel
        {
            Name = parameter.Name.Trim(),
            In = parameter.In.Trim().ToLowerInvariant(),
            Required = parameter.Required,
            Type = normalizedType.Type,
            Nullable = normalizedType.Nullable,
            EnumValues = enumValues,
            ReferenceId = NormalizeOptionalText(parameter.ReferenceId),
            Schema = NormalizeOptionalSchema(
                parameter.Schema,
                schemasByName,
                new HashSet<string>(StringComparer.Ordinal))
        };
    }

    // islevi: Istege ait medya tipi ve sema adresini kararli bicime getirir.
    private static SpecRequestBodyModel NormalizeRequestBody(
        SpecRequestBodyModel body,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName)
    {
        return new SpecRequestBodyModel
        {
            Required = body.Required,
            MediaType = body.MediaType.Trim().ToLowerInvariant(),
            SchemaReferenceId = NormalizeOptionalText(body.SchemaReferenceId),
            Schema = NormalizeOptionalSchema(
                body.Schema,
                schemasByName,
                new HashSet<string>(StringComparer.Ordinal))
        };
    }

    // islevi: Yanit kimligini ve header yuzeyini kararli bicime getirir.
    private static SpecResponseModel NormalizeResponse(
        SpecResponseModel response,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName)
    {
        return new SpecResponseModel
        {
            StatusCode = response.StatusCode.Trim(),
            MediaType = response.MediaType.Trim().ToLowerInvariant(),
            SchemaReferenceId = NormalizeOptionalText(response.SchemaReferenceId),
            Schema = response.Schema == null
                ? null
                : NormalizeSchema(response.Schema, schemasByName, new HashSet<string>(StringComparer.Ordinal)),
            Headers = response.Headers
                .Select(NormalizeHeader)
                .OrderBy(header => header.Name, StringComparer.Ordinal)
                .ToList(),
            Links = response.Links
                .Select(NormalizeOperationLink)
                .OrderBy(link => link.Name, StringComparer.Ordinal)
                .ToList()
        };
    }

    // OpenAPI link hedefini ve parametre expression'larini kararli siraya getirir.
    private static SpecOperationLinkModel NormalizeOperationLink(SpecOperationLinkModel link)
    {
        return new SpecOperationLinkModel
        {
            Name = link.Name.Trim(),
            TargetOperationId = NormalizeOptionalText(link.TargetOperationId),
            TargetOperationReference = NormalizeOptionalText(link.TargetOperationReference),
            ParameterExpressions = link.ParameterExpressions
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .ToDictionary(
                    item => item.Key.Trim(),
                    item => item.Value.Trim(),
                    StringComparer.Ordinal)
        };
    }

    // islevi: Header tip/null sozlesmesini kararli bicime getirir.
    private static SpecHeaderModel NormalizeHeader(SpecHeaderModel header)
    {
        var normalizedType = NormalizeType(header.Type, header.Nullable);
        return new SpecHeaderModel
        {
            Name = header.Name.Trim(),
            Required = header.Required,
            Type = normalizedType.Type,
            Nullable = normalizedType.Nullable,
            ReferenceId = NormalizeOptionalText(header.ReferenceId),
            Example = NormalizeOptionalText(header.Example)
        };
    }

    // islevi: Sema referanslarini cozer, allOf parcalarini birlestirir ve ozellikleri kararli siraya dizer.
    private static SpecSchemaModel NormalizeSchema(
        SpecSchemaModel schema,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName,
        HashSet<string> resolvingReferences)
    {
        var propertyIndex = new Dictionary<string, SpecSchemaPropertyModel>(StringComparer.Ordinal);
        var normalizedType = NormalizeType(schema.Type, schema.Nullable);
        var enumValues = NormalizeEnumValues(schema.EnumValues);

        if (TryResolveSchema(schema.ReferenceId, schemasByName, resolvingReferences, out var referencedSchema))
        {
            var normalizedReference = NormalizeSchema(referencedSchema!, schemasByName, resolvingReferences);
            AddProperties(propertyIndex, normalizedReference.Properties);
            normalizedType = PreferOwnType(normalizedType, normalizedReference);
            enumValues = PreferOwnEnum(enumValues, normalizedReference.EnumValues);
            resolvingReferences.Remove(schema.ReferenceId!);
        }

        foreach (var allOfSchema in schema.AllOf)
        {
            var normalizedAllOf = NormalizeSchema(allOfSchema, schemasByName, resolvingReferences);
            AddProperties(propertyIndex, normalizedAllOf.Properties);
            normalizedType = PreferOwnType(normalizedType, normalizedAllOf);
            enumValues = PreferOwnEnum(enumValues, normalizedAllOf.EnumValues);
        }

        AddProperties(
            propertyIndex,
            schema.Properties.Select(property => NormalizeSchemaProperty(property, schemasByName, resolvingReferences)));

        var normalized = new SpecSchemaModel
        {
            Name = schema.Name.Trim(),
            ReferenceId = NormalizeOptionalText(schema.ReferenceId),
            IsInternal = schema.IsInternal,
            Type = normalizedType.Type,
            Nullable = normalizedType.Nullable,
            Format = NormalizeOptionalText(schema.Format),
            MinLength = schema.MinLength,
            MaxLength = schema.MaxLength,
            Pattern = schema.Pattern,
            Minimum = schema.Minimum,
            Maximum = schema.Maximum,
            MinItems = schema.MinItems,
            MaxItems = schema.MaxItems,
            UniqueItems = schema.UniqueItems,
            AllowAdditionalProperties = schema.AllowAdditionalProperties,
            EnumValues = enumValues,
            Properties = propertyIndex.Values
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .ToList(),
            AllOf = new List<SpecSchemaModel>()
        };

        AddNestedSchemas(normalized, schema, schemasByName, resolvingReferences);
        return normalized;
    }

    // Dizi, ek property ve alternatif semalari ana semayla ayni referans kurallariyla normalize eder.
    private static void AddNestedSchemas(
        SpecSchemaModel target,
        SpecSchemaModel source,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName,
        HashSet<string> resolvingReferences)
    {
        target.Items = NormalizeOptionalSchema(source.Items, schemasByName, resolvingReferences);
        target.AdditionalProperties = NormalizeOptionalSchema(
            source.AdditionalProperties,
            schemasByName,
            resolvingReferences);
        target.AnyOf = NormalizeSchemaList(source.AnyOf, schemasByName, resolvingReferences);
        target.OneOf = NormalizeSchemaList(source.OneOf, schemasByName, resolvingReferences);
        target.Not = NormalizeOptionalSchema(source.Not, schemasByName, resolvingReferences);
    }

    // Opsiyonel alt semayi ziyaret durumunu koruyarak normalize eder.
    private static SpecSchemaModel? NormalizeOptionalSchema(
        SpecSchemaModel? schema,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName,
        HashSet<string> resolvingReferences)
    {
        return schema == null ? null : NormalizeSchema(schema, schemasByName, resolvingReferences);
    }

    // Alternatif sema listesini kaynak sirasini koruyarak normalize eder.
    private static List<SpecSchemaModel> NormalizeSchemaList(
        IEnumerable<SpecSchemaModel> schemas,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName,
        HashSet<string> resolvingReferences)
    {
        return schemas
            .Select(schema => NormalizeSchema(schema, schemasByName, resolvingReferences))
            .ToList();
    }

    // islevi: Cozum dongusune girmeden yerel component referansini bulur ve ziyaret durumunu isaretler.
    private static bool TryResolveSchema(
        string? referenceId,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName,
        HashSet<string> resolvingReferences,
        out SpecSchemaModel? schema)
    {
        schema = null;
        if (string.IsNullOrWhiteSpace(referenceId) || !resolvingReferences.Add(referenceId))
        {
            return false;
        }

        if (schemasByName.TryGetValue(referenceId, out schema))
        {
            return true;
        }

        resolvingReferences.Remove(referenceId);
        return false;
    }

    // islevi: Kaynak sema ozelliklerini ada gore birlestirir; daha yakin tanim oncekini ezer.
    private static void AddProperties(
        IDictionary<string, SpecSchemaPropertyModel> propertyIndex,
        IEnumerable<SpecSchemaPropertyModel> properties)
    {
        foreach (var property in properties)
        {
            propertyIndex[property.Name] = property;
        }
    }

    // islevi: Yerel semada tip varsa onu, yoksa cozulmus semanin tipini kullanir.
    private static (string? Type, bool Nullable) PreferOwnType(
        (string? Type, bool Nullable) ownType,
        SpecSchemaModel inheritedSchema)
    {
        return ownType.Type == null
            ? (inheritedSchema.Type, ownType.Nullable || inheritedSchema.Nullable)
            : (ownType.Type, ownType.Nullable || inheritedSchema.Nullable);
    }

    // islevi: Yerel semada enum varsa onu, yoksa cozulmus semanin enum degerlerini kullanir.
    private static List<string> PreferOwnEnum(List<string> ownValues, List<string> inheritedValues)
    {
        return ownValues.Count > 0 ? ownValues : inheritedValues.ToList();
    }

    // islevi: Sema ozelligini normalize eder ve varsa component referansindan tip/enum bilgisini tamamlar.
    private static SpecSchemaPropertyModel NormalizeSchemaProperty(
        SpecSchemaPropertyModel property,
        IReadOnlyDictionary<string, SpecSchemaModel> schemasByName,
        HashSet<string> resolvingReferences)
    {
        var normalizedType = NormalizeType(property.Type, property.Nullable);
        var enumValues = NormalizeEnumValues(property.EnumValues);

        if (TryResolveSchema(property.ReferenceId, schemasByName, resolvingReferences, out var referencedSchema))
        {
            var normalizedReference = NormalizeSchema(referencedSchema!, schemasByName, resolvingReferences);
            normalizedType = PreferOwnType(normalizedType, normalizedReference);
            enumValues = PreferOwnEnum(enumValues, normalizedReference.EnumValues);
            resolvingReferences.Remove(property.ReferenceId!);
        }

        return new SpecSchemaPropertyModel
        {
            Name = property.Name.Trim(),
            Type = normalizedType.Type,
            Nullable = normalizedType.Nullable,
            Required = property.Required,
            ReadOnly = property.ReadOnly,
            EnumValues = enumValues,
            ReferenceId = NormalizeOptionalText(property.ReferenceId),
            Schema = property.Schema == null
                ? null
                : NormalizeSchema(property.Schema, schemasByName, resolvingReferences)
        };
    }

    // islevi: OAS 3.0 nullable ile OAS 3.1 null tip birlesimini tek tip/null ciftine indirger.
    private static (string? Type, bool Nullable) NormalizeType(string? type, bool nullable)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return (null, nullable);
        }

        var types = TypeSeparatorRegex()
            .Split(type)
            .Select(item => item.Trim().ToLowerInvariant())
            .Where(item => item.Length > 0)
            .ToList();
        var acceptsNull = nullable || types.RemoveAll(
            item => item == SpecNormalizationTextConstants.Normalization.NullType) > 0;
        var normalized = string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            types.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal));

        return (normalized.Length == 0 ? null : normalized, acceptsNull);
    }

    // islevi: Enum degerlerini siradan bagimsiz ve tekrarsiz hale getirir.
    private static List<string> NormalizeEnumValues(IEnumerable<string> values)
    {
        return values
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
    }

    // islevi: Dokumantasyon metinlerini korurken satir sonu ve bicim bosluklarini yalanci fark olmaktan cikarir.
    private static SpecDocumentationModel NormalizeDocumentation(SpecDocumentationModel documentation)
    {
        return new SpecDocumentationModel
        {
            TargetKind = documentation.TargetKind.Trim(),
            Target = NormalizePath(documentation.Target),
            Summary = NormalizeOptionalText(documentation.Summary, normalizeWhitespace: true),
            Description = NormalizeOptionalText(documentation.Description, normalizeWhitespace: true),
            Example = NormalizeOptionalText(documentation.Example, normalizeWhitespace: true)
        };
    }

    // islevi: Opsiyonel metni trim eder ve istendiginde tum whitespace'i tek bosluga indirger.
    private static string? NormalizeOptionalText(string? value, bool normalizeWhitespace = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return normalizeWhitespace
            ? WhitespaceRegex().Replace(value, SpecNormalizationTextConstants.Normalization.SingleSpace).Trim()
            : value.Trim();
    }
}
