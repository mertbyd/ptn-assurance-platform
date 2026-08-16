using System.Text.Json;
using System.Text.Json.Nodes;
using NSubstitute;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Volo.Abp.Settings;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Ptn.ApiContractChecker.Conformance;

// islevi: Request assertion, minimal ornek ve ODG onerilerinin deterministik sinirlarini kanitlar.
public class ConformanceAuthoring_Tests
{
    [Fact]
    public async Task Missing_Required_Query_Should_Fail_Request_Assertion()
    {
        var fixture = BuildFixture(BuildAuthoringSnapshot());
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        var manager = new ResponseConformanceManager(
            new OperationResolver(), fixture.Resolver, new ConformancePolicyResolver(),
            new ConformanceSettingsResolver(settings));

        var result = await manager.AssertRequestAsync(fixture.Entity, new RequestConformanceRequest(
            "target", "POST", "/orders", new Dictionary<string, string>(),
            new Dictionary<string, string>(), "application/json",
            JsonDocument.Parse("{\"state\":\"open\"}").RootElement.Clone(),
            ConformanceProfileCodes.Strict));

        result.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.ResponseSchemaViolation);
        result.Violations.Single().JsonPointer.ShouldBe("/query/customerId");
    }

    [Fact]
    public async Task Request_Example_Should_Contain_Only_Required_Placeholders_Within_2Kb()
    {
        var fixture = BuildFixture(BuildAuthoringSnapshot());
        var builder = new RequestExampleBuilder(fixture.Resolver, new OperationResolver());

        var result = await builder.BuildAsync(
            fixture.Entity,
            new OperationSelectionRequest("target", "POST", "/orders"));

        result.ValuesArePlaceholders.ShouldBeTrue();
        result.Query.ShouldContainKey("customerId");
        result.Body!.AsObject().ShouldContainKey("state");
        result.Body.AsObject().ShouldNotContainKey("note");
        result.MeasureUtf8Bytes().ShouldBeLessThanOrEqualTo(2048);
    }

    [Fact]
    public async Task Binding_Suggestions_Should_Use_Name_And_Type_Only_And_Be_Limited()
    {
        var fixture = BuildFixture(BuildAuthoringSnapshot());
        var suggester = new OperationBindingSuggester(fixture.Resolver, new OperationResolver());

        var result = await suggester.SuggestAsync(
            fixture.Entity,
            new OperationSelectionRequest("target", "POST", "/orders"));

        result.Suggestions.Count.ShouldBeLessThanOrEqualTo(5);
        result.Suggestions.Single().Bindings.ShouldContain(binding =>
            binding.SourcePointer == "/body/customerId" && binding.TargetPointer == "/query/customerId");
        result.MeasureUtf8Bytes().ShouldBeLessThanOrEqualTo(2048);
    }

    [Fact]
    public async Task Request_Example_Should_Skip_ReadOnly_Use_One_Array_Item_And_Cap_Depth()
    {
        var fixture = BuildFixture(BuildAuthoringSnapshot());
        var result = await new RequestExampleBuilder(fixture.Resolver, new OperationResolver())
            .BuildAsync(fixture.Entity, new OperationSelectionRequest("target", "POST", "/orders"));
        var body = result.Body!.AsObject();

        body.ShouldNotContainKey("serverValue");
        body["tags"]!.AsArray().Count.ShouldBe(1);
        body["level1"]!["level2"]!["level3"]!.AsObject().Count.ShouldBe(0);
    }

    [Fact]
    public async Task Binding_Suggestion_Should_Rank_Id_To_ResourceId_First()
    {
        var model = BuildAuthoringSnapshot();
        model.Operations[0].Responses[0].Schema!.Properties[0].Name = "id";
        model.Operations[1].Parameters[0].Name = "orderId";
        var fixture = BuildFixture(model);

        var result = await new OperationBindingSuggester(fixture.Resolver, new OperationResolver())
            .SuggestAsync(fixture.Entity, new OperationSelectionRequest("target", "POST", "/orders"));

        result.Suggestions.First().Bindings.First().SourcePointer.ShouldBe("/body/id");
        result.Suggestions.First().Bindings.First().TargetPointer.ShouldBe("/query/orderId");
    }

    [Fact]
    public async Task Assertion_Derivability_Should_Reject_Missing_And_Warn_Optional_Deterministically()
    {
        var fixture = BuildFixture(BuildAuthoringSnapshot());
        var manager = new AssertionDerivabilityManager(fixture.Resolver, new OperationResolver());
        var request = new AssertionDerivabilityRequest
        {
            OperationId = "target",
            Method = "POST",
            Path = "/orders",
            AssertionPaths = ["/missing", "/note", "/id"]
        };

        var first = await manager.ValidateAsync(fixture.Entity, request);
        var second = await manager.ValidateAsync(fixture.Entity, request);

        first.Assertions.Single(item => item.JsonPointer == "/missing").OutcomeCode
            .ShouldBe(AssertionDerivabilityCodes.AssertionNotInContract);
        first.Assertions.Single(item => item.JsonPointer == "/note").OutcomeCode
            .ShouldBe(AssertionDerivabilityCodes.DerivableButOptional);
        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
        first.MeasureUtf8Bytes().ShouldBeLessThanOrEqualTo(ConformanceAuthoringConstants.MaxAssertionResultBytes);
    }

    [Fact]
    public async Task Operation_Summary_Should_Stay_Under_2Kb_And_Store_Full_Result()
    {
        var model = BuildAuthoringSnapshot();
        var response = model.Operations[1].Responses[0];
        response.Schema!.Properties = Enumerable.Range(0, 60)
            .Select(index => new SpecSchemaPropertyModel
                { Name = $"field{index:D2}", Type = "string", Required = true })
            .ToList();
        var fixture = BuildFixture(model);
        var store = BuildResultStore();
        var manager = new SpecSnapshotAuthoringManager(fixture.Resolver, new OperationResolver(), store);

        var result = await manager.FindOperationAsync(
            fixture.Entity,
            new OperationSelectionRequest("target", "POST", "/orders"),
            SnapshotVerbosityCodes.Minimal);

        result.MeasureUtf8Bytes().ShouldBeLessThanOrEqualTo(2048);
        result.ResultRef.ShouldNotBeNull();
        store.Find(result.ResultRef!)!.Operation!.ResponseFields.Count.ShouldBe(60);
        JsonSerializer.Serialize(result).ShouldNotContain("openapi");
    }

    private static Fixture BuildFixture(SpecSnapshotModel model)
    {
        var content = new SpecContent(Guid.NewGuid(), "raw", "canonical", "{}", 2, "application/json", null);
        var resolver = Substitute.For<ISpecSchemaResolver>();
        resolver.GetSnapshotAsync(content).Returns(model);
        var entity = new SpecSnapshot(
            Guid.NewGuid(), Guid.NewGuid(), content.Id, Guid.NewGuid(), "1", DateTime.UtcNow, null);
        typeof(SpecSnapshot).GetProperty(nameof(SpecSnapshot.SpecContent))!.SetValue(entity, content);
        return new Fixture(resolver, entity);
    }

    private static SpecSnapshotModel BuildAuthoringSnapshot()
    {
        return new SpecSnapshotModel
        {
            Operations =
            [
                new SpecOperationModel
                {
                    OperationId = "source",
                    Method = "GET",
                    Path = "/customers/{id}",
                    Responses =
                    [
                        new SpecResponseModel
                        {
                            StatusCode = "200",
                            MediaType = "application/json",
                            Schema = new SpecSchemaModel
                            {
                                Properties =
                                [
                                    new SpecSchemaPropertyModel { Name = "customerId", Type = "string" }
                                ]
                            }
                        }
                    ]
                },
                BuildTargetOperation()
            ]
        };
    }

    private static SpecOperationModel BuildTargetOperation()
    {
        return new SpecOperationModel
        {
            OperationId = "target",
            Method = "POST",
            Path = "/orders",
            Parameters =
            [
                new SpecParameterModel { Name = "customerId", In = "query", Type = "string", Required = true }
            ],
            RequestBodies =
            [
                new SpecRequestBodyModel
                {
                    Required = true,
                    MediaType = "application/json",
                    Schema = new SpecSchemaModel
                    {
                        Type = "object",
                        Properties =
                        [
                            new SpecSchemaPropertyModel
                            {
                                Name = "state",
                                Type = "string",
                                Required = true,
                                EnumValues = ["\"open\""]
                            },
                            new SpecSchemaPropertyModel { Name = "note", Type = "string" }
                            ,new SpecSchemaPropertyModel
                            {
                                Name = "serverValue", Type = "string", Required = true, ReadOnly = true
                            },
                            new SpecSchemaPropertyModel
                            {
                                Name = "tags", Type = "array", Required = true,
                                Schema = new SpecSchemaModel
                                    { Type = "array", Items = new SpecSchemaModel { Type = "string" } }
                            },
                            BuildNestedProperty("level1", 3)
                        ]
                    }
                }
            ],
            Responses =
            [
                new SpecResponseModel
                {
                    StatusCode = "201",
                    MediaType = "application/json",
                    Schema = new SpecSchemaModel
                    {
                        Type = "object",
                        Properties =
                        [
                            new SpecSchemaPropertyModel { Name = "id", Type = "string", Required = true },
                            new SpecSchemaPropertyModel { Name = "note", Type = "string" }
                        ]
                    }
                }
            ]
        };
    }

    private static SpecSchemaPropertyModel BuildNestedProperty(string name, int remainingDepth)
    {
        var schema = new SpecSchemaModel { Type = "object" };
        if (remainingDepth > 0)
        {
            schema.Properties.Add(BuildNestedProperty($"level{4 - remainingDepth + 1}", remainingDepth - 1));
        }

        return new SpecSchemaPropertyModel { Name = name, Type = "object", Required = true, Schema = schema };
    }

    private static SnapshotAuthoringResultStore BuildResultStore()
    {
        var tenant = Substitute.For<ICurrentTenant>();
        var user = Substitute.For<ICurrentUser>();
        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(Guid.Parse("00000000-0000-0000-0000-000000000123"));
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return new SnapshotAuthoringResultStore(
            new MemoryCache(new MemoryCacheOptions()), tenant, user, guidGenerator, settings);
    }

    private sealed record Fixture(ISpecSchemaResolver Resolver, SpecSnapshot Entity);
}
