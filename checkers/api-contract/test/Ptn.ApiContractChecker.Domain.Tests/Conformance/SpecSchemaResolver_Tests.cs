using Microsoft.Extensions.Caching.Memory;
using NJsonSchema.Validation;
using NSubstitute;
using Ptn.ApiContractChecker.Constants.Formats.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Formats;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Formats;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.ApiContractChecker.Conformance;

// islevi: Sema resolver'in ref/allOf, canonical cache ve dialect davranisini kanitlar.
public class SpecSchemaResolver_Tests
{
    [Fact]
    public async Task Ref_AllOf_Should_Flatten_And_Canonical_Hash_Should_Cache()
    {
        var reader = Substitute.For<ISpecDocumentReader>();
        reader.ReadAsync(Arg.Any<byte[]>()).Returns(BuildParsed(SpecFormatCodes.OpenApi30));
        var resolver = BuildResolver(reader);
        var content = BuildContent();
        var snapshot = await resolver.GetSnapshotAsync(content);
        var operation = snapshot.Operations.Single();

        var resolved = await resolver.ResolveAsync(content, operation, "200", "application/json");
        await resolver.GetSnapshotAsync(content);

        resolved.ShouldNotBeNull();
        resolved.SchemaNode.Validate("{\"id\":1}", resolved.SchemaType, new JsonSchemaValidatorSettings())
            .ShouldBeEmpty();
        await reader.Received(1).ReadAsync(Arg.Any<byte[]>());
    }

    [Fact]
    public async Task OpenApi31_Nullable_Should_Accept_Null()
    {
        var component = new OpenApi31SchemaDialectComponent();
        var resolved = await component.BuildAsync(new SpecSchemaModel
        {
            Type = "string",
            Nullable = true
        });

        resolved.SchemaNode.Validate("null", resolved.SchemaType, new JsonSchemaValidatorSettings())
            .ShouldBeEmpty();
    }

    private static SpecSchemaResolver BuildResolver(ISpecDocumentReader reader)
    {
        var components = new ISpecSchemaDialectComponent[]
        {
            new Swagger20SchemaDialectComponent(),
            new OpenApi30SchemaDialectComponent(),
            new OpenApi31SchemaDialectComponent()
        };
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return new SpecSchemaResolver(
            new MemoryCache(new MemoryCacheOptions()),
            reader,
            new SpecSnapshotNormalizer(),
            new SpecFormatComponentResolver<ISpecSchemaDialectComponent>(components),
            settings);
    }

    private static ParsedSpecModel BuildParsed(string formatCode)
    {
        var snapshot = new SpecSnapshotModel
        {
            Schemas =
            [
                new SpecSchemaModel
                {
                    Name = "Base",
                    Type = "object",
                    Properties =
                    [
                        new SpecSchemaPropertyModel
                        {
                            Name = "id",
                            Type = "integer",
                            Required = true,
                            Schema = new SpecSchemaModel { Type = "integer" }
                        }
                    ]
                }
            ],
            Operations =
            [
                new SpecOperationModel
                {
                    Method = "GET",
                    Path = "/items/{}",
                    Responses =
                    [
                        new SpecResponseModel
                        {
                            StatusCode = "200",
                            MediaType = "application/json",
                            Schema = new SpecSchemaModel
                            {
                                AllOf = [new SpecSchemaModel { ReferenceId = "Base" }]
                            }
                        }
                    ]
                }
            ]
        };
        return new ParsedSpecModel(formatCode, "1", "{}", snapshot);
    }

    private static SpecContent BuildContent()
    {
        return new SpecContent(Guid.NewGuid(), "raw", "canonical", "{}", 2, "application/json", null);
    }
}
