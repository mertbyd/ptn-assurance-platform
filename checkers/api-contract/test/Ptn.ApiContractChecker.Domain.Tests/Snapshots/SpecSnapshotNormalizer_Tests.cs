using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Snapshots;

// islevi: RULE-0006'daki surum, referans, sira, path, dokumantasyon ve whitespace farklarinin elendigini kanitlar.
// sistemdeki gorevi: Diff motoruna yalniz deterministik model ulasmasini saglayan saf normalizer regresyon kapisidir.
public class SpecSnapshotNormalizer_Tests
{
    private readonly SpecSnapshotNormalizer _normalizer = new();

    // OAS 3.0 nullable ile OAS 3.1 null tip birlesiminin ayni modele indigini kanitlar.
    [Fact]
    public void Nullable_Flag_And_Null_Type_Union_Should_Normalize_Equally()
    {
        var openApi30 = BuildSnapshotWithProperty(new SpecSchemaPropertyModel
        {
            Name = "name",
            Type = "string",
            Nullable = true
        });
        var openApi31 = BuildSnapshotWithProperty(new SpecSchemaPropertyModel
        {
            Name = "name",
            Type = "string|null"
        });

        SerializeStructure(_normalizer.Normalize(openApi30))
            .ShouldBe(SerializeStructure(_normalizer.Normalize(openApi31)));
    }

    // allOf ve ref ile kurulan semanin duz semayla ayni ozellikleri urettigini ve ref kimligini korudugunu kanitlar.
    [Fact]
    public void AllOf_And_References_Should_Normalize_To_The_Flat_Schema()
    {
        var composed = BuildComposedSnapshot();
        var flat = BuildFlatSnapshot();

        var normalizedComposed = _normalizer.Normalize(composed);
        var normalizedFlat = _normalizer.Normalize(flat);
        var composedChild = normalizedComposed.Schemas.Single(schema => schema.Name == "Child");
        var flatChild = normalizedFlat.Schemas.Single(schema => schema.Name == "Child");

        JsonSerializer.Serialize(composedChild).ShouldBe(JsonSerializer.Serialize(flatChild));
        composedChild.Properties.Single(property => property.Name == "state").ReferenceId.ShouldBe("State");
    }

    // Property, enum, tag ve security listelerinin giris sirasindan bagimsiz oldugunu kanitlar.
    [Fact]
    public void Structural_Lists_Should_Normalize_In_A_Deterministic_Order()
    {
        var first = BuildOrderedSnapshot(reverse: false);
        var second = BuildOrderedSnapshot(reverse: true);

        SerializeStructure(_normalizer.Normalize(first))
            .ShouldBe(SerializeStructure(_normalizer.Normalize(second)));
    }

    // Path parametresi adinin endpoint kimligini degistirmedigini kanitlar.
    [Fact]
    public void Path_Parameter_Names_Should_Be_Masked()
    {
        var first = BuildSnapshotWithPath("/users/{id}");
        var second = BuildSnapshotWithPath("/users/{userId}");

        SerializeStructure(_normalizer.Normalize(first))
            .ShouldBe(SerializeStructure(_normalizer.Normalize(second)));
    }

    // Description degisikliginin yapisal modeli degistirmeden DocsOnly olarak korundugunu kanitlar.
    [Fact]
    public void Description_Change_Should_Be_Kept_Outside_The_Structural_Model()
    {
        var first = BuildSnapshotWithDocumentation("Lists users");
        var second = BuildSnapshotWithDocumentation("Returns every user");
        var normalizedFirst = _normalizer.Normalize(first);
        var normalizedSecond = _normalizer.Normalize(second);

        SerializeStructure(normalizedFirst).ShouldBe(SerializeStructure(normalizedSecond));
        normalizedFirst.Documentation.Single().IsDocumentationOnly.ShouldBeTrue();
        normalizedFirst.Documentation.Single().Description
            .ShouldNotBe(normalizedSecond.Documentation.Single().Description);
    }

    // Girinti, satir sonu ve ardisik bosluk farkinin dokumantasyon modelini bile degistirmedigini kanitlar.
    [Fact]
    public void Whitespace_And_Line_Endings_Should_Have_No_Effect()
    {
        var first = BuildSnapshotWithDocumentation("Lists\r\n  all   users");
        var second = BuildSnapshotWithDocumentation("Lists all users");

        JsonSerializer.Serialize(_normalizer.Normalize(first))
            .ShouldBe(JsonSerializer.Serialize(_normalizer.Normalize(second)));
    }

    // Tek property'li sema fotografini test girdisi olarak kurar.
    private static SpecSnapshotModel BuildSnapshotWithProperty(SpecSchemaPropertyModel property)
    {
        return new SpecSnapshotModel
        {
            Schemas = new List<SpecSchemaModel>
            {
                new() { Name = "User", Properties = new List<SpecSchemaPropertyModel> { property } }
            }
        };
    }

    // Ref ve allOf kullanan sema fotografini kurar.
    private static SpecSnapshotModel BuildComposedSnapshot()
    {
        return new SpecSnapshotModel
        {
            Schemas = new List<SpecSchemaModel>
            {
                new()
                {
                    Name = "State",
                    Type = "string",
                    EnumValues = new List<string> { "\"active\"", "\"passive\"" }
                },
                new()
                {
                    Name = "Base",
                    Properties = new List<SpecSchemaPropertyModel>
                    {
                        new() { Name = "id", Type = "string", Required = true },
                        new() { Name = "state", ReferenceId = "State" }
                    }
                },
                new()
                {
                    Name = "Child",
                    AllOf = new List<SpecSchemaModel>
                    {
                        new() { ReferenceId = "Base" },
                        new()
                        {
                            Properties = new List<SpecSchemaPropertyModel>
                            {
                                new() { Name = "name", Type = "string" }
                            }
                        }
                    }
                }
            }
        };
    }

    // allOf sonucunun acikca yazilmis duz sema fotografini kurar.
    private static SpecSnapshotModel BuildFlatSnapshot()
    {
        var snapshot = BuildComposedSnapshot();
        snapshot.Schemas[^1] = new SpecSchemaModel
        {
            Name = "Child",
            Properties = new List<SpecSchemaPropertyModel>
            {
                new() { Name = "name", Type = "string" },
                new() { Name = "state", ReferenceId = "State" },
                new() { Name = "id", Type = "string", Required = true }
            }
        };
        return snapshot;
    }

    // Sirasi terslenebilen operasyon, security ve sema listelerini kurar.
    private static SpecSnapshotModel BuildOrderedSnapshot(bool reverse)
    {
        var tags = reverse ? new[] { "users", "public" } : new[] { "public", "users" };
        var scopes = reverse ? new[] { "write", "read" } : new[] { "read", "write" };
        var properties = reverse
            ? new[]
            {
                new SpecSchemaPropertyModel { Name = "zeta", Type = "string", EnumValues = new List<string> { "b", "a" } },
                new SpecSchemaPropertyModel { Name = "alpha", Type = "integer" }
            }
            : new[]
            {
                new SpecSchemaPropertyModel { Name = "alpha", Type = "integer" },
                new SpecSchemaPropertyModel { Name = "zeta", Type = "string", EnumValues = new List<string> { "a", "b" } }
            };

        return new SpecSnapshotModel
        {
            Operations = new List<SpecOperationModel>
            {
                new()
                {
                    Path = "/users",
                    Method = "get",
                    Tags = tags.ToList(),
                    SecurityRequirements = new List<SpecSecurityRequirementModel>
                    {
                        new()
                        {
                            Schemes = new List<SpecSecuritySchemeModel>
                            {
                                new() { Name = "oauth", Scopes = scopes.ToList() }
                            }
                        }
                    }
                }
            },
            Schemas = new List<SpecSchemaModel>
            {
                new() { Name = "User", Properties = properties.ToList() }
            }
        };
    }

    // Tek operasyonlu path fotografini test girdisi olarak kurar.
    private static SpecSnapshotModel BuildSnapshotWithPath(string path)
    {
        return new SpecSnapshotModel
        {
            Operations = new List<SpecOperationModel>
            {
                new() { Path = path, Method = "GET" }
            }
        };
    }

    // Yapidan ayri tek description kaydi tasiyan fotografi kurar.
    private static SpecSnapshotModel BuildSnapshotWithDocumentation(string description)
    {
        var snapshot = BuildSnapshotWithPath("/users/{id}");
        snapshot.Documentation.Add(new SpecDocumentationModel
        {
            TargetKind = SpecNormalizationTextConstants.DocumentationTargets.Operation,
            Target = "GET /users/{id}",
            Description = description
        });
        return snapshot;
    }

    // Yalniz diff'e girecek yapisal koleksiyonlari kararli JSON metnine cevirir.
    private static string SerializeStructure(SpecSnapshotModel snapshot)
    {
        return JsonSerializer.Serialize(new
        {
            snapshot.Operations,
            snapshot.Schemas
        });
    }
}
