using System.Text.Json;
using NSubstitute;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Snapshots;
using Ptn.ApiContractChecker.Settings;
using Shouldly;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.ApiContractChecker.Conformance;

// islevi: Sema siniri, sistematik negatif vaka, alan butcesi ve operasyon link adaylarini kanitlar.
// sistemdeki gorevi: KBP-629 mekanik uretim kurallarinin tahmin veya rastgelelikle genislemesini engeller.
public class SampleAndOperationLink_Tests
{
    [Fact]
    public void Boundary_Samples_Should_Produce_Exact_String_Axis_And_Skip_Unconstrained_Field()
    {
        var generator = new BoundarySampleGenerator();
        var samples = generator.Generate("/body/name", new SpecSchemaModel
        {
            Type = "string",
            MinLength = 2,
            MaxLength = 10
        });

        samples.Select(sample => JsonSerializer.Deserialize<string>(sample.Value!)!.Length)
            .ShouldBe([1, 2, 3, 9, 10, 11]);
        generator.Generate("/body/note", new SpecSchemaModel { Type = "string" }).ShouldBeEmpty();
    }

    [Fact]
    public void Boundary_Samples_Should_Produce_Exact_Numeric_Axis()
    {
        var samples = new BoundarySampleGenerator().Generate("/body/amount", new SpecSchemaModel
        {
            Type = "number",
            Minimum = 2,
            Maximum = 10
        });

        samples.Select(sample => JsonSerializer.Deserialize<decimal>(sample.Value!))
            .ShouldBe([1m, 2m, 3m, 9m, 10m, 11m]);
    }

    [Fact]
    public void Negative_Samples_Should_Omit_Each_Required_Field_And_Reject_Every_Constraint()
    {
        var generator = new NegativeSampleGenerator();
        var schema = new SpecSchemaModel
        {
            Type = "string",
            EnumValues = ["\"OPEN\"", "\"CLOSED\""],
            Pattern = "^[A-Z]+$",
            Format = "uuid"
        };
        var samples = generator.Generate("/body/state", schema, true)
            .Concat(generator.Generate("/body/name", new SpecSchemaModel { Type = "string" }, true))
            .ToList();

        samples.Where(sample => sample.ConstraintCode == ConstraintCodes.Required)
            .Select(sample => sample.FieldPointer)
            .ShouldBe(["/body/state", "/body/name"]);
        samples.Select(sample => sample.ConstraintCode).ShouldContain(ConstraintCodes.Enum);
        samples.Select(sample => sample.ConstraintCode).ShouldContain(ConstraintCodes.Pattern);
        samples.Select(sample => sample.ConstraintCode).ShouldContain(ConstraintCodes.Type);
        samples.Select(sample => sample.ConstraintCode).ShouldContain(ConstraintCodes.Format);
        samples.ShouldAllBe(sample => sample.ExpectedOutcomeCode == SampleExpectedOutcomeCodes.ShouldReject);
    }

    [Fact]
    public async Task Sample_Budget_Should_Cap_Each_Field()
    {
        var snapshot = BuildSampleSnapshot();
        var fixture = BuildFixture(snapshot);
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        settings.GetOrNullAsync(ApiContractCheckerSettings.Conformance.ValueRetentionMode)
            .Returns(ValueRetentionModeCodes.Full);
        var manager = new SampleSetManager(
            fixture.Resolver,
            new OperationResolver(),
            new BoundarySampleGenerator(),
            new NegativeSampleGenerator(),
            new ValueRetentionPolicyResolver(settings),
            new FindingValueRedactor());

        var result = await manager.BuildAsync(
            fixture.Entity,
            new SampleSetRequest("createOrder", string.Empty, string.Empty, SampleKindCodes.Both, 2));

        result.Samples.GroupBy(sample => sample.FieldPointer)
            .ShouldAllBe(group => group.Count() <= 2);
    }

    [Fact]
    public async Task Operation_Links_Should_Rank_Declared_And_Keep_Location_And_Schema_Candidates_Human_Approved()
    {
        var fixture = BuildFixture(BuildLinkSnapshot());
        var result = await new OperationLinkSuggester(fixture.Resolver, new OperationResolver())
            .SuggestAsync(fixture.Entity, new OperationLinkRequest("createOrder", 5));

        result.Candidates.First().SourceCode.ShouldBe(OperationLinkSourceCodes.DeclaredLink);
        result.Candidates.First().Score.ShouldBe(SampleGenerationConsts.DeclaredLinkScore);
        result.Candidates.ShouldContain(candidate => candidate.SourceCode == OperationLinkSourceCodes.SchemaMatch);
        result.Candidates.ShouldContain(candidate => candidate.SourceCode == OperationLinkSourceCodes.LocationHeader);
        result.Candidates.First().ParameterMap.Single().SourceResponsePointer.ShouldBe("/body/orderId");
        result.Candidates.ShouldAllBe(candidate => candidate.RequiresHumanApproval);
        result.Candidates.ShouldAllBe(candidate => candidate.Score >= SampleGenerationConsts.LinkScoreThreshold);
    }

    [Fact]
    public async Task Operation_Links_Should_Return_Empty_When_No_Source_Matches()
    {
        var snapshot = BuildLinkSnapshot();
        var source = snapshot.Operations.Single(operation => operation.OperationId == "createOrder");
        source.Responses.Single().Links.Clear();
        source.Responses.Single().Headers.Clear();
        source.Responses.Single().Schema!.Properties.Clear();
        var fixture = BuildFixture(snapshot);

        var result = await new OperationLinkSuggester(fixture.Resolver, new OperationResolver())
            .SuggestAsync(fixture.Entity, new OperationLinkRequest("createOrder", 5));

        result.Candidates.ShouldBeEmpty();
    }

    // Alan butcesi testi icin cok kisitli tek request property'li snapshot kurar.
    private static SpecSnapshotModel BuildSampleSnapshot()
    {
        return new SpecSnapshotModel
        {
            Operations =
            [
                new SpecOperationModel
                {
                    OperationId = "createOrder",
                    Method = "POST",
                    Path = "/orders",
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
                                        Name = "name",
                                        Type = "string",
                                        Required = true,
                                        Schema = new SpecSchemaModel
                                        {
                                            Type = "string",
                                            MinLength = 2,
                                            MaxLength = 10,
                                            Pattern = "^[a-z]+$"
                                        }
                                    }
                                ]
                            }
                        }
                    ]
                }
            ]
        };
    }

    // Uc aday kaynagini ayri hedeflerde tasiyan snapshot kurar.
    private static SpecSnapshotModel BuildLinkSnapshot()
    {
        return new SpecSnapshotModel
        {
            Operations =
            [
                BuildLinkSource(),
                BuildTarget("getOrder", "/orders/{id}", "orderId"),
                BuildTarget("getCustomer", "/customers/{id}", "customerId"),
                BuildTarget("getCreatedOrder", "/created-orders/{id}", "id")
            ]
        };
    }

    // Link, response semasi ve Location ornegi tasiyan kaynak operasyonu kurar.
    private static SpecOperationModel BuildLinkSource()
    {
        return new SpecOperationModel
        {
            OperationId = "createOrder",
            Method = "POST",
            Path = "/orders",
            Responses =
            [
                new SpecResponseModel
                {
                    StatusCode = "201",
                    Schema = new SpecSchemaModel
                    {
                        Properties =
                        [
                            new SpecSchemaPropertyModel { Name = "orderId", Type = "string" },
                            new SpecSchemaPropertyModel { Name = "customerId", Type = "string" }
                        ]
                    },
                    Headers =
                    [
                        new SpecHeaderModel
                        {
                            Name = SampleGenerationConsts.LocationHeaderName,
                            Example = JsonSerializer.Serialize("/created-orders/42")
                        }
                    ],
                    Links =
                    [
                        new SpecOperationLinkModel
                        {
                            Name = "GetOrder",
                            TargetOperationId = "getOrder",
                            ParameterExpressions = new Dictionary<string, string>(StringComparer.Ordinal)
                            {
                                ["orderId"] = "$response.body#/orderId"
                            }
                        }
                    ]
                }
            ]
        };
    }

    // Tek string path parametreli hedef operasyonu kurar.
    private static SpecOperationModel BuildTarget(string operationId, string path, string parameterName)
    {
        return new SpecOperationModel
        {
            OperationId = operationId,
            Method = "GET",
            Path = path,
            Parameters =
            [
                new SpecParameterModel
                {
                    Name = parameterName,
                    In = "path",
                    Type = "string",
                    Required = true
                }
            ]
        };
    }

    // Saf snapshot modelini mevcut entity ve resolver sinirina baglar.
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

    private sealed record Fixture(ISpecSchemaResolver Resolver, SpecSnapshot Entity);
}
