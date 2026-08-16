using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Comparison;

// islevi: KBP-612 sema farklarinin yon, erisilebilirlik ve rename davranislarini kanitlar.
// sistemdeki gorevi: DTO degisikliklerinin yalanci yon, sil-ekle veya kullanilmayan sema bulgusu uretmesini engeller.
public class SpecSchemaComparisonManager_Tests
{
    private readonly SpecSchemaComparisonManager _manager = new(new SpecCollectionComparer());

    // Request semasina eklenen zorunlu property'nin request yonunde raporlandigini kanitlar.
    [Fact]
    public void Required_Request_Property_Addition_Should_Be_Reported()
    {
        var source = Snapshot(Schema("UserRequest"), request: true);
        var target = Snapshot(Schema("UserRequest", Property("email", required: true)), request: true);

        var difference = AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.NewRequiredRequestProperty,
            DifferenceDirectionCodes.Request);

        difference.Address.PropertyPath.ShouldBe("email");
    }

    // Optional request property eklenmesinin required-property bulgusu uretmedigini kanitlar.
    [Fact]
    public void Optional_Request_Property_Addition_Should_Not_Be_Reported_As_Required()
    {
        var source = Snapshot(Schema("UserRequest"), request: true);
        var target = Snapshot(Schema("UserRequest", Property("email")), request: true);

        _manager.Compare(source, target).ShouldBeEmpty();
    }

    // Request property'nin optional durumdan required duruma gecisini kanitlar.
    [Fact]
    public void Request_Property_Became_Required_Should_Be_Reported()
    {
        var source = Snapshot(Schema("UserRequest", Property("email")), request: true);
        var target = Snapshot(Schema("UserRequest", Property("email", required: true)), request: true);

        AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.RequestPropertyBecameRequired,
            DifferenceDirectionCodes.Request);
    }

    // Request property'nin required durumdan optional duruma gecisinin ters bulgu uretmedigini kanitlar.
    [Fact]
    public void Request_Property_Became_Optional_Should_Not_Be_Reported_As_Required()
    {
        var source = Snapshot(Schema("UserRequest", Property("email", required: true)), request: true);
        var target = Snapshot(Schema("UserRequest", Property("email")), request: true);

        _manager.Compare(source, target).ShouldBeEmpty();
    }

    // Request property tipinin string'den integer'a degismesini kanitlar.
    [Fact]
    public void Request_Property_Type_Change_Should_Be_Reported()
    {
        var source = Snapshot(Schema("UserRequest", Property("id")), request: true);
        var target = Snapshot(Schema("UserRequest", Property("id", type: "integer")), request: true);

        var difference = AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.RequestPropertyTypeChanged,
            DifferenceDirectionCodes.Request);

        difference.OldValue.ShouldBe("string");
        difference.NewValue.ShouldBe("integer");
    }

    // Ayni request property tipinin type-changed bulgusu uretmedigini kanitlar.
    [Fact]
    public void Unchanged_Request_Property_Type_Should_Not_Be_Reported()
    {
        var snapshot = Snapshot(Schema("UserRequest", Property("id")), request: true);

        _manager.Compare(snapshot, snapshot).ShouldBeEmpty();
    }

    // Response property'nin required durumdan optional duruma gecisini kanitlar.
    [Fact]
    public void Response_Property_Became_Optional_Should_Be_Reported()
    {
        var source = Snapshot(Schema("UserResponse", Property("email", required: true)), response: true);
        var target = Snapshot(Schema("UserResponse", Property("email")), response: true);

        AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.ResponsePropertyBecameOptional,
            DifferenceDirectionCodes.Response);
    }

    // Response property'nin optional durumdan required duruma gecisinin ters bulgu uretmedigini kanitlar.
    [Fact]
    public void Response_Property_Became_Required_Should_Not_Be_Reported_As_Optional()
    {
        var source = Snapshot(Schema("UserResponse", Property("email")), response: true);
        var target = Snapshot(Schema("UserResponse", Property("email", required: true)), response: true);

        _manager.Compare(source, target).ShouldBeEmpty();
    }

    // Response semasindan kaldirilan zorunlu property'nin alan kaybi olarak raporlandigini kanitlar.
    [Fact]
    public void Removed_Required_Response_Property_Should_Be_Reported()
    {
        var source = Snapshot(Schema("UserResponse", Property("email", required: true)), response: true);
        var target = Snapshot(Schema("UserResponse"), response: true);

        AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.ResponsePropertyBecameOptional,
            DifferenceDirectionCodes.Response);
    }

    // Response property'nin nullable olmasini response yonunde kanitlar.
    [Fact]
    public void Response_Property_Became_Nullable_Should_Be_Reported()
    {
        var source = Snapshot(Schema("UserResponse", Property("email")), response: true);
        var target = Snapshot(Schema("UserResponse", Property("email", nullable: true)), response: true);

        AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.ResponsePropertyBecameNullable,
            DifferenceDirectionCodes.Response);
    }

    // Response property'nin nullable durumdan non-nullable duruma gecisinin ters bulgu uretmedigini kanitlar.
    [Fact]
    public void Response_Property_Became_NonNullable_Should_Not_Be_Reported_As_Nullable()
    {
        var source = Snapshot(Schema("UserResponse", Property("email", nullable: true)), response: true);
        var target = Snapshot(Schema("UserResponse", Property("email")), response: true);

        _manager.Compare(source, target).ShouldBeEmpty();
    }

    // Iki yonde kullanilan semadaki tip degisikliginin request ve response bulgularini birlikte urettigini kanitlar.
    [Fact]
    public void Schema_Used_By_Request_And_Response_Should_Produce_Both_Directions()
    {
        var source = Snapshot(Schema("User", Property("id")), request: true, response: true);
        var target = Snapshot(Schema("User", Property("id", type: "integer")), request: true, response: true);

        var differences = _manager.Compare(source, target);

        differences.Count.ShouldBe(2);
        differences.Select(difference => difference.DirectionCode).ShouldBe(
            new[] { DifferenceDirectionCodes.Request, DifferenceDirectionCodes.Response },
            ignoreOrder: true);
    }

    // Operasyonlardan erisilmeyen degismis semanin bulgu uretmedigini kanitlar.
    [Fact]
    public void Unused_Schema_Change_Should_Not_Be_Reported()
    {
        var source = Snapshot(Schema("Unused", Property("id")));
        var target = Snapshot(Schema("Unused", Property("id", type: "integer")));

        _manager.Compare(source, target).ShouldBeEmpty();
    }

    // Kullanilmayan benzer semanin erisilebilir rename adayini deterministik eslemede tuketmedigini kanitlar.
    [Fact]
    public void Unused_Schema_Should_Not_Consume_Reachable_Rename_Candidate()
    {
        var source = Snapshot(
            new[]
            {
                Schema("UnusedLegacy", Property("id")),
                Schema("UsedLegacy", Property("id"))
            },
            requestSchemaName: "UsedLegacy");
        var target = Snapshot(
            new[] { Schema("ModernRequest", Property("id")) },
            requestSchemaName: "ModernRequest");

        var difference = AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.SchemaRenamed,
            DifferenceDirectionCodes.Request);

        difference.OldValue.ShouldBe("UsedLegacy");
        difference.NewValue.ShouldBe("ModernRequest");
    }

    // Ozdes property kumesine sahip farkli adli semalarin tek rename bulgusu urettigini kanitlar.
    [Fact]
    public void Structurally_Identical_Schema_Rename_Should_Produce_One_Finding()
    {
        var source = Snapshot(Schema("CreateUserRequest", Property("email")), request: true);
        var target = Snapshot(Schema("RegisterUserRequest", Property("email")), request: true);

        var difference = AssertSingleDifference(
            _manager.Compare(source, target),
            DifferenceKindCodes.SchemaRenamed,
            DifferenceDirectionCodes.Request);

        difference.OldValue.ShouldBe("CreateUserRequest");
        difference.NewValue.ShouldBe("RegisterUserRequest");
    }

    // Esik altindaki iki semanin rename yerine ayri silme ve ekleme bulgulari urettigini kanitlar.
    [Fact]
    public void Dissimilar_Schemas_Should_Produce_Remove_And_Add()
    {
        var source = Snapshot(
            Schema("LegacyRequest", Property("id"), Property("name")),
            request: true);
        var target = Snapshot(
            Schema("ModernRequest", Property("code"), Property("status")),
            request: true);

        var differences = _manager.Compare(source, target);

        differences.Count.ShouldBe(2);
        differences.ShouldContain(difference => difference.KindCode == DifferenceKindCodes.SchemaRemoved);
        differences.ShouldContain(difference => difference.KindCode == DifferenceKindCodes.SchemaAdded);
        differences.ShouldNotContain(difference => difference.KindCode == DifferenceKindCodes.SchemaRenamed);
    }

    // Iki ozdes snapshot'in sifir sema farki urettigini kanitlar.
    [Fact]
    public void Identical_Snapshots_Should_Produce_No_Differences()
    {
        var snapshot = Snapshot(
            Schema("User", Property("id", required: true), Property("email", nullable: true)),
            request: true,
            response: true);

        _manager.Compare(snapshot, snapshot).ShouldBeEmpty();
    }

    // Property giris sirasi degisse de bulgu cikti sirasinin ayni kaldigini kanitlar.
    [Fact]
    public void Difference_Order_Should_Be_Deterministic()
    {
        var firstTarget = Snapshot(
            Schema("UserRequest", Property("zeta", required: true), Property("alpha", required: true)),
            request: true);
        var secondTarget = Snapshot(
            Schema("UserRequest", Property("alpha", required: true), Property("zeta", required: true)),
            request: true);
        var source = Snapshot(Schema("UserRequest"), request: true);

        var first = _manager.Compare(source, firstTarget);
        var second = _manager.Compare(source, secondTarget);

        JsonSerializer.Serialize(first).ShouldBe(JsonSerializer.Serialize(second));
    }

    // Test snapshot'ini tek sema ve secilen operasyon yonleriyle kurar.
    private static SpecSnapshotModel Snapshot(
        SpecSchemaModel schema,
        bool request = false,
        bool response = false)
        => Snapshot(
            new[] { schema },
            request ? schema.Name : null,
            response ? schema.Name : null);

    // Test snapshot'ini sema listesi ve yon koklerindeki referans adlariyla kurar.
    private static SpecSnapshotModel Snapshot(
        IReadOnlyCollection<SpecSchemaModel> schemas,
        string? requestSchemaName = null,
        string? responseSchemaName = null)
    {
        var operation = new SpecOperationModel
        {
            OperationId = "compareUser",
            Method = "POST",
            Path = "/users"
        };
        if (requestSchemaName is not null)
        {
            operation.RequestBodies.Add(new SpecRequestBodyModel
            {
                MediaType = "application/json",
                SchemaReferenceId = requestSchemaName
            });
        }

        if (responseSchemaName is not null)
        {
            operation.Responses.Add(new SpecResponseModel
            {
                StatusCode = "200",
                MediaType = "application/json",
                SchemaReferenceId = responseSchemaName
            });
        }

        return new SpecSnapshotModel
        {
            Schemas = schemas.ToList(),
            Operations = requestSchemaName is not null || responseSchemaName is not null
                ? new List<SpecOperationModel> { operation }
                : new List<SpecOperationModel>()
        };
    }

    // Test semasini ad ve property listesiyle kurar.
    private static SpecSchemaModel Schema(string name, params SpecSchemaPropertyModel[] properties)
        => new()
        {
            Name = name,
            Properties = properties.ToList()
        };

    // Test property sozlesmesini secilebilir tip, required ve nullable alanlariyla kurar.
    private static SpecSchemaPropertyModel Property(
        string name,
        string type = "string",
        bool required = false,
        bool nullable = false)
        => new()
        {
            Name = name,
            Type = type,
            Required = required,
            Nullable = nullable
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
