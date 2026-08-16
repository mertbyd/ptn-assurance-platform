using System.Text;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.ExceptionCodes.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Snapshots;

// islevi: Ham spec baytini OpenAPI kutuphanesiyle okuyup format, surum, kanonik metin ve domain snapshot'ina cevirir.
// sistemdeki gorevi: Kutuphane tipini ve surum modelini altyapida tutar; domaine yalniz saf POCO modeller cikar.
public class SpecDocumentReader : ISpecDocumentReader, ITransientDependency
{
    // JSON ve YAML govdelerinin ayni yoldan okunmasini saglayan paylasilan okuma ayari.
    private static readonly OpenApiReaderSettings ReaderSettings = CreateReaderSettings();

    // Govdeyi ayristirir, okunamayan dokumani reddeder ve kanonik metni uretir.
    public async Task<ParsedSpecModel> ReadAsync(byte[] content)
    {
        var result = await LoadAsync(content);

        EnsureParsed(result);
        var document = result.Document!;
        var specVersion = result.Diagnostic!.SpecificationVersion;

        return new ParsedSpecModel(
            ResolveFormatCode(specVersion),
            document.Info?.Version,
            await SerializeCanonicalAsync(document, specVersion),
            BuildSnapshot(document));
    }

    // YAML okuyucusu kayitli olmadan YAML govdesi okunamaz; ayar tek yerde kurulur.
    private static OpenApiReaderSettings CreateReaderSettings()
    {
        // Izlenen servisin kendi spec kalitesi bizim reddetme sebebimiz degildir: bu urun ucuncu taraf
        // dokumanlarini gozler, onlari denetlemez. Bos ruleset ile Errors yalniz okuyucu duzeyinde kalir,
        // yani "eksik cozulmus dokuman yanlis fark uretir" guard'i korunurken stil ihlalleri kabul edilir.
        var settings = new OpenApiReaderSettings
        {
            RuleSet = ValidationRuleSet.GetEmptyRuleSet()
        };
        settings.AddYamlReader();
        return settings;
    }

    // Kutuphane istisnasini alan hata koduna ceviren tek nokta; tanimasiz govde format hatasidir.
    private static async Task<ReadResult> LoadAsync(byte[] content)
    {
        using var stream = new MemoryStream(content);
        try
        {
            return await OpenApiDocument.LoadAsync(stream, settings: ReaderSettings);
        }
        catch (OpenApiUnsupportedSpecVersionException)
        {
            throw new BusinessException(SpecFormatExceptionCodes.UnsupportedFormat);
        }
    }

    // Okunamayan veya eksik cozulmus dokumani snapshot yazilmadan reddeder.
    private static void EnsureParsed(ReadResult result)
    {
        if (result.Document == null || result.Diagnostic == null)
        {
            throw new BusinessException(SpecSnapshotExceptionCodes.ParseFailed);
        }

        if (result.Diagnostic.Errors.Count > 0)
        {
            throw new BusinessException(SpecSnapshotExceptionCodes.DocumentInvalid);
        }
    }

    // Okunan spec surumunu kararli lookup koduna cevirir.
    private static string ResolveFormatCode(OpenApiSpecVersion specVersion)
    {
        return specVersion switch
        {
            OpenApiSpecVersion.OpenApi2_0 => SpecFormatCodes.Swagger20,
            OpenApiSpecVersion.OpenApi3_0 => SpecFormatCodes.OpenApi30,
            OpenApiSpecVersion.OpenApi3_1 => SpecFormatCodes.OpenApi31,
            _ => throw new BusinessException(SpecFormatExceptionCodes.UnsupportedFormat)
        };
    }

    // Dokumani kendi surumunde yeniden seri hale getirerek bicim gurultusunden arinmis metni uretir.
    private static async Task<string> SerializeCanonicalAsync(
        OpenApiDocument document,
        OpenApiSpecVersion specVersion)
    {
        using var stream = new MemoryStream();
        await document.SerializeAsJsonAsync(stream, specVersion);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Okunan dokumani format kutuphanesinden bagimsiz operasyon ve sema fotografina indirger.
    private static SpecSnapshotModel BuildSnapshot(OpenApiDocument document)
    {
        // info.version ve servers kosum ani gercekleridir; var olan CanonicalText/CanonicalHash
        // kimligine ayrica eklenmezler, zaten kanonik dokuman iceriginde bulunurlar.
        var snapshot = new SpecSnapshotModel
        {
            ApiVersion = document.Info?.Version,
            Servers = (document.Servers ?? [])
                .Select(server => server.Url?.Trim())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Cast<string>()
                .ToList()
        };
        AddOperations(snapshot, document);
        AddSchemas(snapshot, document);
        return snapshot;
    }

    // Dokumandaki path ve HTTP metotlarini operasyon modellerine cevirir.
    private static void AddOperations(SpecSnapshotModel snapshot, OpenApiDocument document)
    {
        foreach (var pathEntry in document.Paths)
        {
            foreach (var operationEntry in pathEntry.Value.Operations ?? [])
            {
                snapshot.Operations.Add(BuildOperation(
                    snapshot,
                    document,
                    pathEntry.Key,
                    operationEntry.Key,
                    pathEntry.Value,
                    operationEntry.Value));
            }
        }
    }

    // Tek operasyonun kimlik, parametre, govde, yanit, tag ve guvenlik yuzeyini kurar.
    private static SpecOperationModel BuildOperation(
        SpecSnapshotModel snapshot,
        OpenApiDocument document,
        string path,
        HttpMethod method,
        IOpenApiPathItem pathItem,
        OpenApiOperation operation)
    {
        var operationTarget = string.Concat(
            method.Method,
            SpecNormalizationTextConstants.Normalization.SingleSpace,
            path);
        AddDocumentation(
            snapshot,
            SpecNormalizationTextConstants.DocumentationTargets.Operation,
            operationTarget,
            operation.Summary,
            operation.Description,
            null);

        return new SpecOperationModel
        {
            Path = path,
            Method = method.Method,
            OperationId = operation.OperationId,
            IsInternal = IsInternal(operation.Extensions),
            Tags = (operation.Tags ?? Enumerable.Empty<OpenApiTagReference>())
                .Select(tag => tag.Name ?? string.Empty)
                .ToList(),
            SecurityRequirements = BuildSecurityRequirements(
                operation.Security ?? document.Security ?? []),
            Parameters = BuildParameters(snapshot, operationTarget, pathItem, operation),
            RequestBodies = BuildRequestBodies(snapshot, operationTarget, operation.RequestBody),
            Responses = BuildResponses(snapshot, operationTarget, operation.Responses)
        };
    }

    // Path parametrelerini operasyon parametreleriyle ad ve konuma gore birlestirir.
    private static List<SpecParameterModel> BuildParameters(
        SpecSnapshotModel snapshot,
        string operationTarget,
        IOpenApiPathItem pathItem,
        OpenApiOperation operation)
    {
        var parameterIndex = new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);
        AddParametersToIndex(parameterIndex, pathItem.Parameters ?? []);
        AddParametersToIndex(parameterIndex, operation.Parameters ?? []);

        return parameterIndex.Values
            .Select(parameter => BuildParameter(snapshot, operationTarget, parameter))
            .ToList();
    }

    // Parametre listesini operasyon seviyesinin path seviyesini ezebilecegi kimlik sozluguyle indeksler.
    private static void AddParametersToIndex(
        IDictionary<string, IOpenApiParameter> parameterIndex,
        IEnumerable<IOpenApiParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            var key = string.Join(
                SpecNormalizationTextConstants.Normalization.TypeSeparator,
                parameter.In?.ToString(),
                parameter.Name);
            parameterIndex[key] = parameter;
        }
    }

    // Tek parametreyi tip, null ve referans kimligiyle domain modeline indirger.
    private static SpecParameterModel BuildParameter(
        SpecSnapshotModel snapshot,
        string operationTarget,
        IOpenApiParameter parameter)
    {
        var schema = ResolveSchema(parameter.Schema, parameter.Content);
        var parameterTarget = BuildChildTarget(
            operationTarget,
            parameter.In?.ToString(),
            parameter.Name);
        AddDocumentation(
            snapshot,
            SpecNormalizationTextConstants.DocumentationTargets.Parameter,
            parameterTarget,
            null,
            parameter.Description,
            SerializeExample(parameter.Example));

        return new SpecParameterModel
        {
            Name = parameter.Name ?? string.Empty,
            In = parameter.In?.ToString() ?? string.Empty,
            Required = parameter.Required,
            Type = schema?.Type?.ToString(),
            Nullable = false,
            EnumValues = schema is OpenApiSchemaReference
                ? new List<string>()
                : SerializeEnumValues(schema?.Enum),
            ReferenceId = ResolveSchemaReferenceId(schema),
            Schema = BuildOptionalSchema(snapshot, schema)
        };
    }

    // Istege ait her medya tipini ayri ve karsilastirilabilir govde modeline cevirir.
    private static List<SpecRequestBodyModel> BuildRequestBodies(
        SpecSnapshotModel snapshot,
        string operationTarget,
        IOpenApiRequestBody? requestBody)
    {
        if (requestBody == null)
        {
            return new List<SpecRequestBodyModel>();
        }

        if (requestBody.Content == null || requestBody.Content.Count == 0)
        {
            AddDocumentation(
                snapshot,
                SpecNormalizationTextConstants.DocumentationTargets.RequestBody,
                operationTarget,
                null,
                requestBody.Description,
                null);
            return new List<SpecRequestBodyModel>
            {
                new() { Required = requestBody.Required }
            };
        }

        return requestBody.Content
            .Select(contentEntry => BuildRequestBody(
                snapshot,
                operationTarget,
                requestBody,
                contentEntry))
            .ToList();
    }

    // Tek medya tipindeki istek govdesini sema adresi ve dokumantasyonuyla kurar.
    private static SpecRequestBodyModel BuildRequestBody(
        SpecSnapshotModel snapshot,
        string operationTarget,
        IOpenApiRequestBody requestBody,
        KeyValuePair<string, OpenApiMediaType> contentEntry)
    {
        var target = BuildChildTarget(operationTarget, contentEntry.Key);
        AddDocumentation(
            snapshot,
            SpecNormalizationTextConstants.DocumentationTargets.RequestBody,
            target,
            null,
            requestBody.Description,
            SerializeExample(contentEntry.Value.Example));
        return new SpecRequestBodyModel
        {
            Required = requestBody.Required,
            MediaType = contentEntry.Key,
            SchemaReferenceId = ResolveSchemaReferenceId(contentEntry.Value.Schema),
            Schema = BuildOptionalSchema(snapshot, contentEntry.Value.Schema)
        };
    }

    // Yanitlari durum kodu ve medya tipi kimligiyle domain modellerine cevirir.
    private static List<SpecResponseModel> BuildResponses(
        SpecSnapshotModel snapshot,
        string operationTarget,
        OpenApiResponses? responses)
    {
        var models = new List<SpecResponseModel>();
        foreach (var responseEntry in responses ?? [])
        {
            if (responseEntry.Value.Content == null || responseEntry.Value.Content.Count == 0)
            {
                models.Add(BuildResponse(
                    snapshot,
                    operationTarget,
                    responseEntry.Key,
                    string.Empty,
                    responseEntry.Value,
                    null));
                continue;
            }

            foreach (var contentEntry in responseEntry.Value.Content)
            {
                models.Add(BuildResponse(
                    snapshot,
                    operationTarget,
                    responseEntry.Key,
                    contentEntry.Key,
                    responseEntry.Value,
                    contentEntry.Value));
            }
        }

        return models;
    }

    // Tek yaniti sema, header ve dokumantasyon bilgisiyle kurar.
    private static SpecResponseModel BuildResponse(
        SpecSnapshotModel snapshot,
        string operationTarget,
        string statusCode,
        string mediaType,
        IOpenApiResponse response,
        OpenApiMediaType? content)
    {
        var responseTarget = BuildChildTarget(operationTarget, statusCode, mediaType);
        AddDocumentation(
            snapshot,
            SpecNormalizationTextConstants.DocumentationTargets.Response,
            responseTarget,
            null,
            response.Description,
            SerializeExample(content?.Example));

        return new SpecResponseModel
        {
            StatusCode = statusCode,
            MediaType = mediaType,
            SchemaReferenceId = ResolveSchemaReferenceId(content?.Schema),
            Schema = BuildOptionalSchema(snapshot, content?.Schema),
            Headers = BuildHeaders(snapshot, responseTarget, response.Headers),
            Links = BuildLinks(response.Links)
        };
    }

    // Response link'lerini hedef operasyon ve parametre expression'lariyla saf snapshot modeline indirger.
    private static List<SpecOperationLinkModel> BuildLinks(IDictionary<string, IOpenApiLink>? links)
    {
        return (links ?? new Dictionary<string, IOpenApiLink>())
            .Select(linkEntry => new SpecOperationLinkModel
            {
                Name = linkEntry.Key,
                TargetOperationId = linkEntry.Value.OperationId,
                TargetOperationReference = linkEntry.Value.OperationRef,
                ParameterExpressions = (linkEntry.Value.Parameters ??
                                        new Dictionary<string, RuntimeExpressionAnyWrapper>())
                    .Where(parameter => ResolveLinkExpression(parameter.Value) != null)
                    .ToDictionary(
                        parameter => parameter.Key,
                        parameter => ResolveLinkExpression(parameter.Value)!,
                        StringComparer.Ordinal)
            })
            .ToList();
    }

    // Runtime expression veya literal link parametresini kararli metin temsiline cevirir.
    private static string? ResolveLinkExpression(RuntimeExpressionAnyWrapper value)
    {
        return value.Expression?.Expression ?? value.Any?.ToJsonString();
    }

    // Yanit header'larini ad, tip, null ve referans kimligiyle domain modellerine cevirir.
    private static List<SpecHeaderModel> BuildHeaders(
        SpecSnapshotModel snapshot,
        string responseTarget,
        IDictionary<string, IOpenApiHeader>? headers)
    {
        return (headers ?? new Dictionary<string, IOpenApiHeader>())
            .Select(headerEntry => BuildHeader(snapshot, responseTarget, headerEntry))
            .ToList();
    }

    // Tek response header'ini tip, referans ve dokumantasyon bilgisiyle kurar.
    private static SpecHeaderModel BuildHeader(
        SpecSnapshotModel snapshot,
        string responseTarget,
        KeyValuePair<string, IOpenApiHeader> headerEntry)
    {
        var header = headerEntry.Value;
        var schema = ResolveSchema(header.Schema, header.Content);
        AddDocumentation(
            snapshot,
            SpecNormalizationTextConstants.DocumentationTargets.Header,
            BuildChildTarget(responseTarget, headerEntry.Key),
            null,
            header.Description,
            SerializeExample(header.Example));
        return new SpecHeaderModel
        {
            Name = headerEntry.Key,
            Required = header.Required,
            Type = schema?.Type?.ToString(),
            Nullable = false,
            ReferenceId = ResolveSchemaReferenceId(schema),
            Example = SerializeExample(header.Example)
        };
    }

    // Operasyonda etkili olan security requirement alternatiflerini sema ve scope modellerine indirger.
    private static List<SpecSecurityRequirementModel> BuildSecurityRequirements(
        IEnumerable<OpenApiSecurityRequirement> requirements)
    {
        return requirements.Select(requirement => new SpecSecurityRequirementModel
        {
            Schemes = requirement.Select(scheme => new SpecSecuritySchemeModel
            {
                Name = scheme.Key.Reference.Id ?? string.Empty,
                Scheme = scheme.Key.Scheme,
                Scopes = scheme.Value.ToList()
            }).ToList()
        }).ToList();
    }

    // Components altindaki semalari property, enum, referans ve allOf parcalariyla domain modeline cevirir.
    private static void AddSchemas(SpecSnapshotModel snapshot, OpenApiDocument document)
    {
        foreach (var schemaEntry in document.Components?.Schemas ??
                     new Dictionary<string, IOpenApiSchema>())
        {
            snapshot.Schemas.Add(BuildSchema(snapshot, schemaEntry.Key, schemaEntry.Value));
        }
    }

    // Tek semayi referans dongusune girmeden saf domain modeline indirger.
    private static SpecSchemaModel BuildSchema(
        SpecSnapshotModel snapshot,
        string name,
        IOpenApiSchema schema)
    {
        if (schema is OpenApiSchemaReference reference)
        {
            return new SpecSchemaModel
            {
                Name = name,
                ReferenceId = reference.Reference.Id,
                IsInternal = IsInternal(reference.Extensions)
            };
        }

        AddDocumentation(
            snapshot,
            SpecNormalizationTextConstants.DocumentationTargets.Schema,
            name,
            null,
            schema.Description,
            SerializeExample(schema.Example));

        return new SpecSchemaModel
        {
            Name = name,
            IsInternal = IsInternal(schema.Extensions),
            Type = schema.Type?.ToString(),
            Nullable = false,
            Format = schema.Format,
            MinLength = schema.MinLength,
            MaxLength = schema.MaxLength,
            Pattern = schema.Pattern,
            Minimum = ParseDecimal(schema.Minimum),
            Maximum = ParseDecimal(schema.Maximum),
            MinItems = schema.MinItems,
            MaxItems = schema.MaxItems,
            UniqueItems = schema.UniqueItems ?? false,
            Items = BuildOptionalSchema(snapshot, schema.Items),
            AllowAdditionalProperties = schema.AdditionalPropertiesAllowed,
            AdditionalProperties = BuildOptionalSchema(snapshot, schema.AdditionalProperties),
            EnumValues = SerializeEnumValues(schema.Enum),
            Properties = BuildSchemaProperties(snapshot, name, schema),
            AllOf = (schema.AllOf ?? [])
                .Select(allOf => BuildSchema(snapshot, string.Empty, allOf))
                .ToList(),
            AnyOf = (schema.AnyOf ?? [])
                .Select(anyOf => BuildSchema(snapshot, string.Empty, anyOf))
                .ToList(),
            OneOf = (schema.OneOf ?? [])
                .Select(oneOf => BuildSchema(snapshot, string.Empty, oneOf))
                .ToList(),
            Not = BuildOptionalSchema(snapshot, schema.Not)
        };
    }

    // Opsiyonel inline veya referans semayi ayni saf modele indirger.
    private static SpecSchemaModel? BuildOptionalSchema(
        SpecSnapshotModel snapshot,
        IOpenApiSchema? schema)
    {
        return schema == null ? null : BuildSchema(snapshot, string.Empty, schema);
    }

    // Microsoft.OpenApi 2.11 sayisal schema sinirlarini string tasir; domain decimal'a invariant indirger.
    private static decimal? ParseDecimal(string? value)
    {
        return decimal.TryParse(
            value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;
    }

    // Semanin property sozlesmelerini required listesiyle birlestirerek kurar.
    private static List<SpecSchemaPropertyModel> BuildSchemaProperties(
        SpecSnapshotModel snapshot,
        string schemaName,
        IOpenApiSchema schema)
    {
        return (schema.Properties ?? new Dictionary<string, IOpenApiSchema>())
            .Select(propertyEntry => BuildSchemaProperty(
                snapshot,
                schemaName,
                schema,
                propertyEntry))
            .ToList();
    }

    // Tek sema ozelligini required, tip, enum, ref ve dokumantasyon bilgisiyle kurar.
    private static SpecSchemaPropertyModel BuildSchemaProperty(
        SpecSnapshotModel snapshot,
        string schemaName,
        IOpenApiSchema parentSchema,
        KeyValuePair<string, IOpenApiSchema> propertyEntry)
    {
        var property = propertyEntry.Value;
        AddDocumentation(
            snapshot,
            SpecNormalizationTextConstants.DocumentationTargets.Property,
            BuildChildTarget(schemaName, propertyEntry.Key),
            null,
            property.Description,
            SerializeExample(property.Example));
        return new SpecSchemaPropertyModel
        {
            Name = propertyEntry.Key,
            Type = property is OpenApiSchemaReference ? null : property.Type?.ToString(),
            Nullable = false,
            Required = parentSchema.Required?.Contains(propertyEntry.Key) == true,
            ReadOnly = property.ReadOnly,
            EnumValues = property is OpenApiSchemaReference
                ? new List<string>()
                : SerializeEnumValues(property.Enum),
            ReferenceId = ResolveSchemaReferenceId(property),
            Schema = BuildOptionalSchema(snapshot, property)
        };
    }

    // Sema dogrudan verilmediyse content icindeki ilk kararli medya tipi semasini secer.
    private static IOpenApiSchema? ResolveSchema(
        IOpenApiSchema? schema,
        IDictionary<string, OpenApiMediaType>? content)
    {
        return schema ?? content?
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => entry.Value.Schema)
            .FirstOrDefault(candidate => candidate != null);
    }

    // Sema proxy'sindeki local component kimligini provider modelinden ayirir.
    private static string? ResolveSchemaReferenceId(IOpenApiSchema? schema)
    {
        return (schema as OpenApiSchemaReference)?.Reference.Id;
    }

    // Enum JSON degerlerini tip kaybi olmadan kararli metne cevirir.
    private static List<string> SerializeEnumValues(IEnumerable<JsonNode>? values)
    {
        return values?.Select(value => value.ToJsonString()).ToList() ?? new List<string>();
    }

    // Example JSON degerini dokumantasyon modelinde saklanacak kararli metne cevirir.
    private static string? SerializeExample(JsonNode? example)
    {
        return example?.ToJsonString();
    }

    // x-internal extension'indaki gercek boolean degeri provider tipinden saf modele indirger.
    private static bool IsInternal(IDictionary<string, IOpenApiExtension>? extensions)
    {
        if (extensions == null ||
            !extensions.TryGetValue(ContractCheckScopeCodes.InternalExtensionName, out var extension) ||
            extension is not JsonNodeExtension { Node: JsonValue value })
        {
            return false;
        }

        return value.TryGetValue<bool>(out var isInternal) && isInternal;
    }

    // Dokumantasyon alanlarindan en az biri varsa yapisal modelden ayri DocsOnly kaydi acar.
    private static void AddDocumentation(
        SpecSnapshotModel snapshot,
        string targetKind,
        string target,
        string? summary,
        string? description,
        string? example)
    {
        if (string.IsNullOrWhiteSpace(summary) &&
            string.IsNullOrWhiteSpace(description) &&
            string.IsNullOrWhiteSpace(example))
        {
            return;
        }

        snapshot.Documentation.Add(new SpecDocumentationModel
        {
            TargetKind = targetKind,
            Target = target,
            Summary = summary,
            Description = description,
            Example = example
        });
    }

    // Alt nesne adresini runtime kimlik parcalarindan kararli bicimde kurar.
    private static string BuildChildTarget(string parent, params string?[] children)
    {
        return string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            new[] { parent }.Concat(children.Where(child => child != null)!));
    }
}
