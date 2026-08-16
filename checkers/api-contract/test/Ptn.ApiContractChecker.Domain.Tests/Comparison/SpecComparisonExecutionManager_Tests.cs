using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Comparison;

// islevi: Saf comparison giris noktasinin normalize, diff, siniflandirma ve finding donusum zincirini kanitlar.
// sistemdeki gorevi: Iki ham snapshot'tan ContractCheckRun'a yazilabilir kayipsiz ve deterministik bulgu govdesi uretimini korur.
public class SpecComparisonExecutionManager_Tests
{
    private readonly SpecComparisonExecutionManager _manager = BuildManager();

    // Iki ham snapshot'i uctan uca karsilastirip tur, yon, siddet ve adresleriyle bulguya cevirir.
    [Fact]
    public void Snapshots_Should_Produce_Classified_Findings_With_Exact_Addresses()
    {
        var findings = _manager.Compare(BuildBaseSnapshot(), BuildTargetSnapshot());

        findings.Items.Count.ShouldBe(6);
        AssertFinding(
            findings,
            DifferenceKindCodes.RequestPropertyBecameRequired,
            DifferenceDirectionCodes.Request,
            DifferenceSeverityCodes.Breaking,
            schemaName: "UserRequest",
            propertyPath: "email");
        AssertFinding(
            findings,
            DifferenceKindCodes.ResponsePropertyBecameOptional,
            DifferenceDirectionCodes.Response,
            DifferenceSeverityCodes.Breaking,
            schemaName: "UserResponse",
            propertyPath: "name");
        AssertFinding(
            findings,
            DifferenceKindCodes.ResponsePropertyBecameNullable,
            DifferenceDirectionCodes.Response,
            DifferenceSeverityCodes.Breaking,
            schemaName: "UserResponse",
            propertyPath: "name");
        AssertFinding(
            findings,
            DifferenceKindCodes.RequestBodyBecameRequired,
            DifferenceDirectionCodes.Request,
            DifferenceSeverityCodes.Breaking,
            operationId: "updateUser",
            httpMethod: "POST",
            path: "/users/{}",
            schemaName: "UserRequest",
            mediaType: "application/json");
        AssertFinding(
            findings,
            DifferenceKindCodes.ResponseMediaTypeRemoved,
            DifferenceDirectionCodes.Response,
            DifferenceSeverityCodes.Breaking,
            operationId: "updateUser",
            httpMethod: "POST",
            path: "/users/{}",
            responseStatus: "200",
            mediaType: "application/json");
        AssertFinding(
            findings,
            DifferenceKindCodes.DescriptionChanged,
            DifferenceDirectionCodes.Documentation,
            DifferenceSeverityCodes.DocsOnly,
            httpMethod: "POST",
            path: "/users/{}");
    }

    // Snapshot liste siralari degisse de global finding sirasinin ayni kaldigini kanitlar.
    [Fact]
    public void Finding_Order_Should_Be_Deterministic()
    {
        var baseSnapshot = BuildBaseSnapshot();
        var targetSnapshot = BuildTargetSnapshot();
        var first = _manager.Compare(baseSnapshot, targetSnapshot);
        var reversedBase = ReverseCollections(baseSnapshot);
        var reversedTarget = ReverseCollections(targetSnapshot);
        var second = _manager.Compare(reversedBase, reversedTarget);

        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
    }

    // Execution manager'i ayni comparer'i kullanan mevcut saf pipeline adimlariyla kurar.
    private static SpecComparisonExecutionManager BuildManager()
    {
        var collectionComparer = new SpecCollectionComparer();
        return new SpecComparisonExecutionManager(
            new SpecSnapshotNormalizer(),
            new SpecOperationComparisonManager(collectionComparer),
            new SpecSchemaComparisonManager(collectionComparer),
            new SpecDifferenceSeverityClassifier(),
            collectionComparer,
            new SpecComparisonScopeManager());
    }

    // Daralan request ve gevseyen response yuzeyini tasiyan base snapshot'i kurar.
    private static SpecSnapshotModel BuildBaseSnapshot()
        => BuildSnapshot(
            requestRequired: false,
            requestPropertyRequired: false,
            responsePropertyRequired: true,
            responsePropertyNullable: false,
            responseMediaType: "application/json",
            path: "/users/{id}",
            description: "Updates a user");

    // Daralan request, gevseyen response ve degisen dokumantasyonu tasiyan target snapshot'i kurar.
    private static SpecSnapshotModel BuildTargetSnapshot()
        => BuildSnapshot(
            requestRequired: true,
            requestPropertyRequired: true,
            responsePropertyRequired: false,
            responsePropertyNullable: true,
            responseMediaType: "application/xml",
            path: "/users/{userId}",
            description: "Changes a user");

    // Uctan uca senaryonun operasyon, sema ve dokumantasyon yuzeylerini birlikte kurar.
    private static SpecSnapshotModel BuildSnapshot(
        bool requestRequired,
        bool requestPropertyRequired,
        bool responsePropertyRequired,
        bool responsePropertyNullable,
        string responseMediaType,
        string path,
        string description)
    {
        return new SpecSnapshotModel
        {
            Operations =
            [
                new SpecOperationModel
                {
                    OperationId = "updateUser",
                    Method = "post",
                    Path = path,
                    RequestBodies =
                    [
                        new SpecRequestBodyModel
                        {
                            Required = requestRequired,
                            MediaType = "application/json",
                            SchemaReferenceId = "UserRequest"
                        }
                    ],
                    Responses =
                    [
                        new SpecResponseModel
                        {
                            StatusCode = "200",
                            MediaType = responseMediaType,
                            SchemaReferenceId = "UserResponse"
                        }
                    ]
                }
            ],
            Schemas =
            [
                new SpecSchemaModel
                {
                    Name = "UserRequest",
                    Properties =
                    [
                        new SpecSchemaPropertyModel
                        {
                            Name = "email",
                            Type = "string",
                            Required = requestPropertyRequired
                        }
                    ]
                },
                new SpecSchemaModel
                {
                    Name = "UserResponse",
                    Properties =
                    [
                        new SpecSchemaPropertyModel
                        {
                            Name = "name",
                            Type = "string",
                            Required = responsePropertyRequired,
                            Nullable = responsePropertyNullable
                        }
                    ]
                }
            ],
            Documentation =
            [
                new SpecDocumentationModel
                {
                    TargetKind = SpecNormalizationTextConstants.DocumentationTargets.Operation,
                    Target = $"POST {path}",
                    Description = description
                }
            ]
        };
    }

    // Ayni snapshot verisini ters koleksiyon siralariyla yeniden kurar.
    private static SpecSnapshotModel ReverseCollections(SpecSnapshotModel snapshot)
    {
        snapshot.Operations.Reverse();
        snapshot.Schemas.Reverse();
        snapshot.Documentation.Reverse();
        foreach (var schema in snapshot.Schemas)
        {
            schema.Properties.Reverse();
        }

        return snapshot;
    }

    // Beklenen bulguyu tum dolu adres alanlari ve bos kalmasi gereken diger alanlarla dogrular.
    private static void AssertFinding(
        ContractCheckFindings findings,
        string kindCode,
        string directionCode,
        string severityCode,
        string? operationId = null,
        string? httpMethod = null,
        string? path = null,
        string? schemaName = null,
        string? propertyPath = null,
        string? parameterName = null,
        string? responseStatus = null,
        string? mediaType = null)
    {
        var finding = findings.Items.Single(item => item.KindCode == kindCode);
        finding.DirectionCode.ShouldBe(directionCode);
        finding.SeverityCode.ShouldBe(severityCode);
        finding.Address.OperationId.ShouldBe(operationId);
        finding.Address.HttpMethod.ShouldBe(httpMethod);
        finding.Address.Path.ShouldBe(path);
        finding.Address.SchemaName.ShouldBe(schemaName);
        finding.Address.PropertyPath.ShouldBe(propertyPath);
        finding.Address.ParameterName.ShouldBe(parameterName);
        finding.Address.ResponseStatus.ShouldBe(responseStatus);
        finding.Address.MediaType.ShouldBe(mediaType);
    }
}
