using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Differences;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Comparison;

// islevi: KBP-611 operasyon yuzeyi fark turlerinin pozitif, negatif ve deterministik davranislarini kanitlar.
// sistemdeki gorevi: Normalize snapshot'lar arasinda yalanci endpoint, request, response veya dokumantasyon bulgusu uretilmesini engeller.
public class SpecOperationComparisonManager_Tests
{
    private readonly SpecOperationComparisonManager _manager = new(new SpecCollectionComparer());

    // Target'ta ilk kez gorunen operasyonun endpoint-added olarak raporlandigini kanitlar.
    [Fact]
    public void Added_Endpoint_Should_Be_Reported()
    {
        var differences = _manager.Compare(new SpecSnapshotModel(), BuildSnapshot());

        AssertSingleDifference(
            differences,
            DifferenceKindCodes.EndpointAdded,
            DifferenceDirectionCodes.Endpoint);
    }

    // Ayni operasyonun endpoint-added yalanci farki uretmedigini kanitlar.
    [Fact]
    public void Existing_Endpoint_Should_Not_Be_Reported_As_Added()
    {
        var differences = _manager.Compare(BuildSnapshot(), BuildSnapshot());

        differences.ShouldNotContain(difference => difference.KindCode == DifferenceKindCodes.EndpointAdded);
    }

    // Target'tan kaldirilan operasyonun endpoint-removed olarak raporlandigini kanitlar.
    [Fact]
    public void Removed_Endpoint_Should_Be_Reported()
    {
        var differences = _manager.Compare(BuildSnapshot(), new SpecSnapshotModel());

        AssertSingleDifference(
            differences,
            DifferenceKindCodes.EndpointRemoved,
            DifferenceDirectionCodes.Endpoint);
    }

    // Ayni operasyonun endpoint-removed yalanci farki uretmedigini kanitlar.
    [Fact]
    public void Existing_Endpoint_Should_Not_Be_Reported_As_Removed()
    {
        var differences = _manager.Compare(BuildSnapshot(), BuildSnapshot());

        differences.ShouldNotContain(difference => difference.KindCode == DifferenceKindCodes.EndpointRemoved);
    }

    // Ayni parametrenin target enumundan silinen degerin request yonunde raporlandigini kanitlar.
    [Fact]
    public void Removed_Request_Parameter_Enum_Value_Should_Be_Reported()
    {
        var source = BuildSnapshot(operation => operation.Parameters.Add(BuildStateParameter("active", "passive")));
        var target = BuildSnapshot(operation => operation.Parameters.Add(BuildStateParameter("active")));

        var differences = _manager.Compare(source, target);

        var difference = AssertSingleDifference(
            differences,
            DifferenceKindCodes.RequestParameterEnumValueRemoved,
            DifferenceDirectionCodes.Request);
        difference.OldValue.ShouldBe("passive");
        difference.Address.ParameterName.ShouldBe("state");
    }

    // Target enumuna deger eklenmesinin enum-value-removed farki uretmedigini kanitlar.
    [Fact]
    public void Added_Request_Parameter_Enum_Value_Should_Not_Be_Reported()
    {
        var source = BuildSnapshot(operation => operation.Parameters.Add(BuildStateParameter("active")));
        var target = BuildSnapshot(operation => operation.Parameters.Add(BuildStateParameter("active", "passive")));

        var differences = _manager.Compare(source, target);

        differences.ShouldNotContain(difference =>
            difference.KindCode == DifferenceKindCodes.RequestParameterEnumValueRemoved);
    }

    // Optional request body'nin required olmasinin request yonunde raporlandigini kanitlar.
    [Fact]
    public void Required_Request_Body_Should_Be_Reported()
    {
        var source = BuildSnapshot(operation => operation.RequestBodies.Add(BuildRequestBody(required: false)));
        var target = BuildSnapshot(operation => operation.RequestBodies.Add(BuildRequestBody(required: true)));

        var differences = _manager.Compare(source, target);

        var difference = AssertSingleDifference(
            differences,
            DifferenceKindCodes.RequestBodyBecameRequired,
            DifferenceDirectionCodes.Request);
        difference.OldValue.ShouldBe(SpecComparisonTextConstants.Optional);
        difference.NewValue.ShouldBe(SpecComparisonTextConstants.Required);
    }

    // Required request body'nin optional olmasinin became-required farki uretmedigini kanitlar.
    [Fact]
    public void Optional_Request_Body_Should_Not_Be_Reported_As_Required()
    {
        var source = BuildSnapshot(operation => operation.RequestBodies.Add(BuildRequestBody(required: true)));
        var target = BuildSnapshot(operation => operation.RequestBodies.Add(BuildRequestBody(required: false)));

        var differences = _manager.Compare(source, target);

        differences.ShouldNotContain(difference => difference.KindCode == DifferenceKindCodes.RequestBodyBecameRequired);
    }

    // Target'tan kaldirilan 2xx durum kodunun response yonunde raporlandigini kanitlar.
    [Fact]
    public void Removed_Success_Response_Status_Should_Be_Reported()
    {
        var source = BuildSnapshot(operation => operation.Responses.Add(BuildResponse("200")));
        var target = BuildSnapshot();

        var differences = _manager.Compare(source, target);

        AssertSingleDifference(
            differences,
            DifferenceKindCodes.ResponseSuccessStatusRemoved,
            DifferenceDirectionCodes.Response);
    }

    // Basarisiz durum kodunun kaldirilmasinin success-status-removed farki uretmedigini kanitlar.
    [Fact]
    public void Removed_NonSuccess_Response_Status_Should_Not_Be_Reported()
    {
        var source = BuildSnapshot(operation => operation.Responses.Add(BuildResponse("404")));
        var target = BuildSnapshot();

        var differences = _manager.Compare(source, target);

        differences.ShouldNotContain(difference =>
            difference.KindCode == DifferenceKindCodes.ResponseSuccessStatusRemoved);
    }

    // Ayni response statusundan kaldirilan medya tipinin response yonunde raporlandigini kanitlar.
    [Fact]
    public void Removed_Response_Media_Type_Should_Be_Reported()
    {
        var source = BuildSnapshot(operation =>
        {
            operation.Responses.Add(BuildResponse("200", "application/json"));
            operation.Responses.Add(BuildResponse("200", "application/xml"));
        });
        var target = BuildSnapshot(operation =>
            operation.Responses.Add(BuildResponse("200", "application/json")));

        var differences = _manager.Compare(source, target);

        var difference = AssertSingleDifference(
            differences,
            DifferenceKindCodes.ResponseMediaTypeRemoved,
            DifferenceDirectionCodes.Response);
        difference.Address.MediaType.ShouldBe("application/xml");
    }

    // Response'a medya tipi eklenmesinin media-type-removed farki uretmedigini kanitlar.
    [Fact]
    public void Added_Response_Media_Type_Should_Not_Be_Reported()
    {
        var source = BuildSnapshot(operation =>
            operation.Responses.Add(BuildResponse("200", "application/json")));
        var target = BuildSnapshot(operation =>
        {
            operation.Responses.Add(BuildResponse("200", "application/json"));
            operation.Responses.Add(BuildResponse("200", "application/xml"));
        });

        var differences = _manager.Compare(source, target);

        differences.ShouldNotContain(difference => difference.KindCode == DifferenceKindCodes.ResponseMediaTypeRemoved);
    }

    // Target response'tan kaldirilan zorunlu header'in response yonunde raporlandigini kanitlar.
    [Fact]
    public void Removed_Required_Response_Header_Should_Be_Reported()
    {
        var source = BuildSnapshot(operation => operation.Responses.Add(
            BuildResponse("200", "application/json", BuildHeader(required: true))));
        var target = BuildSnapshot(operation => operation.Responses.Add(
            BuildResponse("200", "application/json")));

        var differences = _manager.Compare(source, target);

        var difference = AssertSingleDifference(
            differences,
            DifferenceKindCodes.RequiredResponseHeaderRemoved,
            DifferenceDirectionCodes.Response);
        difference.OldValue.ShouldBe("X-Trace");
    }

    // Optional response header'in kaldirilmasinin required-header-removed farki uretmedigini kanitlar.
    [Fact]
    public void Removed_Optional_Response_Header_Should_Not_Be_Reported()
    {
        var source = BuildSnapshot(operation => operation.Responses.Add(
            BuildResponse("200", "application/json", BuildHeader(required: false))));
        var target = BuildSnapshot(operation => operation.Responses.Add(
            BuildResponse("200", "application/json")));

        var differences = _manager.Compare(source, target);

        differences.ShouldNotContain(difference =>
            difference.KindCode == DifferenceKindCodes.RequiredResponseHeaderRemoved);
    }

    // Yalniz description degisikliginin tek documentation yonlu fark urettigini kanitlar.
    [Fact]
    public void Changed_Description_Should_Produce_One_Documentation_Difference()
    {
        var source = BuildSnapshot(documentation: "Lists users");
        var target = BuildSnapshot(documentation: "Returns users");

        var differences = _manager.Compare(source, target);

        var difference = AssertSingleDifference(
            differences,
            DifferenceKindCodes.DescriptionChanged,
            DifferenceDirectionCodes.Documentation);
        difference.OldValue.ShouldBe("Lists users");
        difference.NewValue.ShouldBe("Returns users");
    }

    // Ayni description metninin documentation farki uretmedigini kanitlar.
    [Fact]
    public void Unchanged_Description_Should_Not_Be_Reported()
    {
        var source = BuildSnapshot(documentation: "Lists users");
        var target = BuildSnapshot(documentation: "Lists users");

        var differences = _manager.Compare(source, target);

        differences.ShouldNotContain(difference => difference.KindCode == DifferenceKindCodes.DescriptionChanged);
    }

    // Normalize path sayesinde path parametre adinin endpoint sil-ekle ciftine donusmedigini kanitlar.
    [Fact]
    public void Renamed_Path_Parameter_Should_Not_Create_Endpoint_Add_Remove()
    {
        var source = BuildSnapshot(operation => operation.Parameters.Add(BuildParameter(name: "id")));
        var target = BuildSnapshot(operation => operation.Parameters.Add(BuildParameter(name: "userId")));

        var differences = _manager.Compare(source, target);

        differences.ShouldBeEmpty();
    }

    // Iki ozdes normalize snapshot'in hic fark uretmedigini kanitlar.
    [Fact]
    public void Identical_Snapshots_Should_Produce_No_Differences()
    {
        var snapshot = BuildSnapshot(operation =>
        {
            operation.Parameters.Add(BuildStateParameter("active"));
            operation.RequestBodies.Add(BuildRequestBody(required: false));
            operation.Responses.Add(BuildResponse("200", "application/json", BuildHeader(required: true)));
        }, "Lists users");

        _manager.Compare(snapshot, snapshot).ShouldBeEmpty();
    }

    // Koleksiyon giris sirasi degisse de ayni farklarin ayni sirada donduruldugunu kanitlar.
    [Fact]
    public void Difference_Order_Should_Be_Deterministic()
    {
        var firstSource = BuildSnapshotWithOperations("zeta", "alpha");
        var secondSource = BuildSnapshotWithOperations("alpha", "zeta");
        var target = new SpecSnapshotModel();

        var first = _manager.Compare(firstSource, target);
        var second = _manager.Compare(secondSource, target);

        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
    }

    // Tek operasyonlu normalize snapshot'i ve varsa operasyon ayrintilarini kurar.
    private static SpecSnapshotModel BuildSnapshot(
        Action<SpecOperationModel>? configure = null,
        string? documentation = null)
    {
        var operation = BuildOperation();
        configure?.Invoke(operation);
        var snapshot = new SpecSnapshotModel
        {
            Operations = new List<SpecOperationModel> { operation }
        };

        if (documentation is not null)
        {
            snapshot.Documentation.Add(new SpecDocumentationModel
            {
                TargetKind = SpecNormalizationTextConstants.DocumentationTargets.Operation,
                Target = "GET /users/{}",
                Description = documentation
            });
        }

        return snapshot;
    }

    // Verilen operationId'lerle operasyon listesi kurar.
    private static SpecSnapshotModel BuildSnapshotWithOperations(params string[] operationIds)
        => new()
        {
            Operations = operationIds.Select(operationId => BuildOperation(operationId)).ToList()
        };

    // Normalize metod/path ve kararli kimlik tasiyan operasyon kurar.
    private static SpecOperationModel BuildOperation(string operationId = "listUsers")
        => new()
        {
            OperationId = operationId,
            Method = "GET",
            Path = "/users/{}"
        };

    // Test icin query parametresi ve enum yuzeyi kurar.
    private static SpecParameterModel BuildStateParameter(
        params string[] enumValues)
        => BuildParameter("state", enumValues);

    // Test icin adlandirilabilir query parametresi ve enum yuzeyi kurar.
    private static SpecParameterModel BuildParameter(
        string name,
        params string[] enumValues)
        => new()
        {
            Name = name,
            In = "query",
            Type = "string",
            EnumValues = enumValues.ToList()
        };

    // Test icin application/json request body kurar.
    private static SpecRequestBodyModel BuildRequestBody(bool required)
        => new()
        {
            Required = required,
            MediaType = "application/json",
            SchemaReferenceId = "UserRequest"
        };

    // Test icin status, medya tipi ve header yuzeyi tasiyan response kurar.
    private static SpecResponseModel BuildResponse(
        string status,
        string mediaType = "application/json",
        params SpecHeaderModel[] headers)
        => new()
        {
            StatusCode = status,
            MediaType = mediaType,
            Headers = headers.ToList()
        };

    // Test icin zorunlulugu secilebilen response header kurar.
    private static SpecHeaderModel BuildHeader(bool required)
        => new()
        {
            Name = "X-Trace",
            Required = required,
            Type = "string"
        };

    // Tek farkin beklenen kapali tur ve yon kodlarini tasidigini dogrular.
    private static SpecDifferenceModel AssertSingleDifference(
        IReadOnlyCollection<SpecDifferenceModel> differences,
        string kindCode,
        string directionCode)
    {
        var difference = differences.ShouldHaveSingleItem();
        difference.KindCode.ShouldBe(kindCode);
        difference.DirectionCode.ShouldBe(directionCode);
        return difference;
    }
}
