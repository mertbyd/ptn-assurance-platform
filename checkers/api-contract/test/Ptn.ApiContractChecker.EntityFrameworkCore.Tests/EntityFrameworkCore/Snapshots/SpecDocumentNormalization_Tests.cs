using System.Text;
using System.Text.Json;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Xunit;
using Ptn.ApiContractChecker.EntityFrameworkCore.Adapters.Snapshots;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Snapshots;

// islevi: Gercek OpenAPI okuyucusunun uc spec surumunu ayni domain snapshot'ina indirgedigini dogrular.
// sistemdeki gorevi: Provider modeli ile saf POCO normalizer arasindaki surumler-arasi regresyon kapisidir.
[Collection(EfCoreIntegrationCollection.Name)]
public class SpecDocumentNormalization_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private const string Swagger20Document = """
        {
          "swagger": "2.0",
          "info": { "title": "Users", "version": "1.0.0" },
          "consumes": ["application/json"],
          "produces": ["application/json"],
          "paths": {
            "/users/{id}": {
              "get": {
                "operationId": "getUser",
                "parameters": [
                  { "name": "id", "in": "path", "required": true, "type": "string", "enum": ["current", "legacy"] },
                  { "name": "body", "in": "body", "required": true, "schema": { "$ref": "#/definitions/UserRequest" } }
                ],
                "responses": {
                  "200": {
                    "description": "ok",
                    "schema": { "$ref": "#/definitions/UserResponse" },
                    "headers": { "X-Trace": { "type": "string" } }
                  }
                }
              }
            }
          },
          "definitions": {
            "UserRequest": {
              "type": "object",
              "required": ["name"],
              "properties": { "name": { "type": "string" } }
            },
            "UserResponse": {
              "type": "object",
              "required": ["id", "name"],
              "properties": { "id": { "type": "string" }, "name": { "type": "string" } }
            }
          }
        }
        """;

    private const string OpenApi30Document = """
        {
          "openapi": "3.0.3",
          "info": { "title": "Users", "version": "1.0.0" },
          "paths": {
            "/users/{id}": {
              "get": {
                "operationId": "getUser",
                "parameters": [
                  { "name": "id", "in": "path", "required": true, "schema": { "type": "string", "enum": ["current", "legacy"] } }
                ],
                "requestBody": {
                  "required": true,
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserRequest" } } }
                },
                "responses": {
                  "200": {
                    "description": "ok",
                    "headers": { "X-Trace": { "schema": { "type": "string" } } },
                    "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserResponse" } } }
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "UserRequest": {
                "type": "object",
                "required": ["name"],
                "properties": { "name": { "type": "string" } }
              },
              "UserResponse": {
                "type": "object",
                "required": ["id", "name"],
                "properties": { "id": { "type": "string" }, "name": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string OpenApi31Document = """
        {
          "openapi": "3.1.0",
          "info": { "title": "Users", "version": "1.0.0" },
          "paths": {
            "/users/{id}": {
              "get": {
                "operationId": "getUser",
                "parameters": [
                  { "name": "id", "in": "path", "required": true, "schema": { "type": "string", "enum": ["current", "legacy"] } }
                ],
                "requestBody": {
                  "required": true,
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserRequest" } } }
                },
                "responses": {
                  "200": {
                    "description": "ok",
                    "headers": { "X-Trace": { "schema": { "type": "string" } } },
                    "content": { "application/json": { "schema": { "$ref": "#/components/schemas/UserResponse" } } }
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "UserRequest": {
                "type": "object",
                "required": ["name"],
                "properties": { "name": { "type": "string" } }
              },
              "UserResponse": {
                "type": "object",
                "required": ["id", "name"],
                "properties": { "id": { "type": "string" }, "name": { "type": "string" } }
              }
            }
          }
        }
        """;

    private readonly ISpecDocumentReader _reader;
    private readonly SpecSnapshotNormalizer _normalizer;
    private readonly SpecOperationComparisonManager _comparisonManager;

    // Gercek okuyucu ile saf normalizer'i test DI konteynerinden alir.
    public SpecDocumentNormalization_Tests()
    {
        _reader = GetRequiredService<ISpecDocumentReader>();
        _normalizer = GetRequiredService<SpecSnapshotNormalizer>();
        _comparisonManager = GetRequiredService<SpecOperationComparisonManager>();
    }

    // Ayni API'nin Swagger 2.0, OAS 3.0 ve OAS 3.1 belgelerinin ayni normalize modeli urettigini kanitlar.
    [Fact]
    public async Task Equivalent_Api_Across_Three_Spec_Versions_Should_Normalize_Equally()
    {
        var swagger20 = await ReadNormalizedAsync(Swagger20Document);
        var openApi30 = await ReadNormalizedAsync(OpenApi30Document);
        var openApi31 = await ReadNormalizedAsync(OpenApi31Document);

        AssertCompleteProjection(swagger20);
        SerializeStructure(swagger20).ShouldBe(SerializeStructure(openApi30));
        SerializeStructure(openApi30).ShouldBe(SerializeStructure(openApi31));
        _comparisonManager.Compare(openApi30, openApi31).ShouldBeEmpty();
    }

    // JSON girintisi ve satir sonu farkinin yapisal snapshot'a hic etki etmedigini kanitlar.
    [Fact]
    public async Task Formatting_Only_Difference_Should_Not_Change_The_Normalized_Model()
    {
        var compact = OpenApi31Document.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("  ", string.Empty);
        var formatted = await ReadNormalizedAsync(OpenApi31Document);
        var unformatted = await ReadNormalizedAsync(compact);

        JsonSerializer.Serialize(formatted).ShouldBe(JsonSerializer.Serialize(unformatted));
    }

    // x-internal true extension'inin provider modelinden operation ve schema runtime bayraklarina tasindigini kanitlar.
    [Fact]
    public async Task Internal_Extension_Should_Be_Projected_To_The_Runtime_Snapshot()
    {
        var document = """
                       {
                         "openapi": "3.0.3",
                         "info": { "title": "Internal", "version": "1" },
                         "paths": {
                           "/internal": {
                             "get": {
                               "x-internal": true,
                               "responses": { "200": { "description": "ok" } }
                             }
                           }
                         },
                         "components": {
                           "schemas": {
                             "InternalDto": { "type": "object", "x-internal": true }
                           }
                         }
                       }
                       """;

        var snapshot = await ReadNormalizedAsync(document);

        snapshot.Operations.Single().IsInternal.ShouldBeTrue();
        snapshot.Schemas.Single().IsInternal.ShouldBeTrue();
    }

    // Constraint, OpenAPI links ve Location orneginin provider sinirinda kaybolmadigini kanitlar.
    [Fact]
    public async Task Sample_And_Link_Fixture_Should_Project_Mechanical_Evidence()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "EntityFrameworkCore",
            "Snapshots",
            "Fixtures",
            "sample-generation-links.json");
        var snapshot = await ReadNormalizedAsync(await File.ReadAllTextAsync(path));
        var source = snapshot.Operations.Single(operation => operation.OperationId == "createOrder");
        var name = source.RequestBodies.Single().Schema!.Properties.Single(property => property.Name == "name");
        var response = source.Responses.Single(item => item.StatusCode == "201");

        name.Schema!.MinLength.ShouldBe(2);
        name.Schema.MaxLength.ShouldBe(10);
        response.Links.Single().TargetOperationId.ShouldBe("getOrder");
        response.Links.Single().ParameterExpressions["orderId"].ShouldBe("$response.body#/orderId");
        response.Headers.Single(header => header.Name == "Location").Example
            .ShouldBe("\"/created-orders/42\"");
    }

    // Esitlik kontrolunun bos modelle gecmemesi icin tum istenen operasyon ve sema yuzeyini dogrular.
    private static void AssertCompleteProjection(SpecSnapshotModel snapshot)
    {
        var operation = snapshot.Operations.Single();
        operation.Path.ShouldBe("/users/{}");
        operation.Method.ShouldBe("GET");
        operation.Parameters.Single().Type.ShouldBe("string");
        operation.Parameters.Single().EnumValues.ShouldBe(new[] { "\"current\"", "\"legacy\"" });
        operation.RequestBodies.Single().SchemaReferenceId.ShouldBe("UserRequest");
        operation.Responses.Single().SchemaReferenceId.ShouldBe("UserResponse");
        operation.Responses.Single().Headers.Single().Name.ShouldBe("X-Trace");
        snapshot.Schemas.Count.ShouldBe(2);
        snapshot.Schemas.Single(schema => schema.Name == "UserResponse").Properties.Count.ShouldBe(2);
        snapshot.Documentation.ShouldContain(documentation => documentation.IsDocumentationOnly);
    }

    // Okuyucu ve normalizer zincirini tek test yardimcisinda calistirir.
    private async Task<SpecSnapshotModel> ReadNormalizedAsync(string document)
    {
        var parsed = await _reader.ReadAsync(Encoding.UTF8.GetBytes(document));
        return _normalizer.Normalize(parsed.Snapshot);
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
