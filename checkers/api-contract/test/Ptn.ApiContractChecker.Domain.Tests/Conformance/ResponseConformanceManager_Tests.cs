using System.Text.Json;
using NJsonSchema;
using NSubstitute;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.ApiContractChecker.Conformance;

// islevi: Response oracle'inin kapali outcome, pointer, belirsizlik, profil ve butce davranisini kanitlar.
public class ResponseConformanceManager_Tests
{
    [Fact]
    public async Task Undocumented_Status_Should_Return_Closed_Outcome()
    {
        var fixture = await BuildFixtureAsync();
        var result = await fixture.Manager.AssertResponseAsync(
            fixture.Entity,
            BuildRequest(statusCode: 201));

        result.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.StatusCodeUndocumented);
    }

    [Fact]
    public async Task Schema_Violation_Should_Return_Pointer_And_Keyword_Without_Value()
    {
        var fixture = await BuildFixtureAsync();
        var result = await fixture.Manager.AssertResponseAsync(
            fixture.Entity,
            BuildRequest(body: "{\"id\":\"bad\"}"));

        result.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.ResponseSchemaViolation);
        result.Violations.ShouldContain(item => item.JsonPointer == "/id" && item.Keyword == "type");
        JsonSerializer.Serialize(result).ShouldNotContain("bad");
    }

    [Fact]
    public void Overlapping_Path_Templates_Should_Not_Resolve()
    {
        var snapshot = new SpecSnapshotModel
        {
            Operations =
            [
                new SpecOperationModel { Method = "GET", Path = "/orders/{id}" },
                new SpecOperationModel { Method = "GET", Path = "/orders/active" }
            ]
        };

        new OperationResolver().Resolve(snapshot, null, "GET", "/orders/active").ShouldBeNull();
    }

    [Fact]
    public async Task Additional_Property_Should_Fail_Strict_And_Be_Suppressed_Lenient()
    {
        var fixture = await BuildFixtureAsync(allowAdditionalProperties: false);
        var strict = await fixture.Manager.AssertResponseAsync(
            fixture.Entity,
            BuildRequest(body: "{\"id\":1,\"extra\":true}", profile: ConformanceProfileCodes.Strict));
        var lenient = await fixture.Manager.AssertResponseAsync(
            fixture.Entity,
            BuildRequest(body: "{\"id\":1,\"extra\":true}", profile: ConformanceProfileCodes.Lenient));

        strict.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.UndocumentedProperty);
        lenient.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.PolicySuppressed);
    }

    [Fact]
    public async Task Same_Input_Should_Be_Deterministic_And_Fit_512_Bytes()
    {
        var fixture = await BuildFixtureAsync(requiredHeaderCount: 12);
        var request = BuildRequest();

        var first = await fixture.Manager.AssertResponseAsync(fixture.Entity, request);
        var second = await fixture.Manager.AssertResponseAsync(fixture.Entity, request);

        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
        first.MeasureUtf8Bytes().ShouldBeLessThanOrEqualTo(512);
    }

    private static async Task<Fixture> BuildFixtureAsync(
        bool allowAdditionalProperties = true,
        int requiredHeaderCount = 0)
    {
        var content = new SpecContent(Guid.NewGuid(), "raw", "canonical", "{}", 2, "application/json", null);
        var responseSchema = BuildSchema(allowAdditionalProperties);
        var snapshot = BuildSnapshot(responseSchema, requiredHeaderCount);
        var schemaResolver = Substitute.For<ISpecSchemaResolver>();
        schemaResolver.GetSnapshotAsync(content).Returns(snapshot);
        var resolved = await new OpenApi30SchemaDialectComponent().BuildAsync(responseSchema);
        schemaResolver.ResolveAsync(content, Arg.Any<SpecOperationModel>(), "200", "application/json")
            .Returns(resolved);
        var manager = BuildManager(schemaResolver);
        return new Fixture(manager, BuildEntity(content));
    }

    private static ResponseConformanceManager BuildManager(ISpecSchemaResolver resolver)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return new ResponseConformanceManager(
            new OperationResolver(),
            resolver,
            new ConformancePolicyResolver(),
            new ConformanceSettingsResolver(settings));
    }

    private static SpecSnapshotModel BuildSnapshot(SpecSchemaModel schema, int requiredHeaderCount)
    {
        return new SpecSnapshotModel
        {
            Operations =
            [
                new SpecOperationModel
                {
                    Method = "GET",
                    Path = "/items/{id}",
                    Responses =
                    [
                        new SpecResponseModel
                        {
                            StatusCode = "200",
                            MediaType = "application/json",
                            Schema = schema,
                            Headers = Enumerable.Range(0, requiredHeaderCount)
                                .Select(index => new SpecHeaderModel { Name = $"x-required-{index}", Required = true })
                                .ToList()
                        }
                    ]
                }
            ]
        };
    }

    private static SpecSchemaModel BuildSchema(bool allowAdditionalProperties)
    {
        return new SpecSchemaModel
        {
            Type = "object",
            AllowAdditionalProperties = allowAdditionalProperties,
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
        };
    }

    private static ResponseConformanceRequest BuildRequest(
        int statusCode = 200,
        string body = "{\"id\":1}",
        string profile = ConformanceProfileCodes.Strict)
    {
        return new ResponseConformanceRequest(
            null,
            "GET",
            "/items/1",
            statusCode,
            "application/json; charset=utf-8",
            new Dictionary<string, string>(),
            JsonDocument.Parse(body).RootElement.Clone(),
            profile);
    }

    private static SpecSnapshot BuildEntity(SpecContent content)
    {
        var entity = new SpecSnapshot(
            Guid.NewGuid(), Guid.NewGuid(), content.Id, Guid.NewGuid(), "1", DateTime.UtcNow, null);
        typeof(SpecSnapshot).GetProperty(nameof(SpecSnapshot.SpecContent))!.SetValue(entity, content);
        return entity;
    }

    private sealed record Fixture(ResponseConformanceManager Manager, SpecSnapshot Entity);
}
