using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Comparison;

// islevi: Kapali fark katalogundaki her tur ve yon kombinasyonunun RULE-0007 siddetini kanitlar.
// sistemdeki gorevi: Yeni veya yanlis yonlu bir DifferenceKind'in sessizce non-breaking sinifina dusmesini engeller.
public class SpecDifferenceSeverityClassifier_Tests
{
    private static readonly IReadOnlyCollection<(string Kind, string Direction, string Severity)> Cases =
    [
        (DifferenceKindCodes.NewRequiredRequestProperty, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.RequestPropertyBecameRequired, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.RequestPropertyTypeChanged, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.RequestPropertyTypeChanged, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.RequestParameterEnumValueRemoved, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.RequestBodyBecameRequired, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.ResponsePropertyBecameOptional, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.ResponsePropertyBecameNullable, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.ResponseSuccessStatusRemoved, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.RequiredResponseHeaderRemoved, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.ResponseMediaTypeRemoved, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.EndpointAdded, DifferenceDirectionCodes.Endpoint, DifferenceSeverityCodes.NonBreaking),
        (DifferenceKindCodes.EndpointRemoved, DifferenceDirectionCodes.Endpoint, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.SchemaAdded, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.NonBreaking),
        (DifferenceKindCodes.SchemaAdded, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.NonBreaking),
        (DifferenceKindCodes.SchemaRemoved, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.SchemaRemoved, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.SchemaRenamed, DifferenceDirectionCodes.Request, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.SchemaRenamed, DifferenceDirectionCodes.Response, DifferenceSeverityCodes.Breaking),
        (DifferenceKindCodes.DescriptionChanged, DifferenceDirectionCodes.Documentation, DifferenceSeverityCodes.DocsOnly)
    ];

    private readonly SpecDifferenceSeverityClassifier _classifier = new();

    // xUnit'e kapali katalogdaki tum gecerli tur, yon ve siddet vakalarini saglar.
    public static IEnumerable<object[]> ClassificationCases => Cases.Select(item => new object[]
    {
        item.Kind,
        item.Direction,
        item.Severity
    });

    // Her katalog turunun yon asimetrisine uygun tek kararli siddet koduna siniflandirildigini kanitlar.
    [Theory]
    [MemberData(nameof(ClassificationCases))]
    public void Difference_Should_Be_Classified_By_Kind_And_Direction(
        string kindCode,
        string directionCode,
        string expectedSeverityCode)
    {
        var difference = BuildDifference(kindCode, directionCode);

        _classifier.Classify(difference).ShouldBe(expectedSeverityCode);
    }

    // DifferenceKind kataloguna eklenen her kodun classifier switch'inde test vakasi bulunmasini zorunlu kilar.
    [Fact]
    public void Classification_Cases_Should_Cover_The_Closed_Difference_Kind_Catalog()
    {
        Cases.Select(item => item.Kind)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ShouldBe(DifferenceKindCodes.All.OrderBy(item => item, StringComparer.Ordinal));
    }

    // Ture ait olmayan yonun gecersiz ara model olarak reddedildigini kanitlar.
    [Fact]
    public void Invalid_Direction_For_Kind_Should_Be_Rejected()
    {
        var difference = BuildDifference(
            DifferenceKindCodes.EndpointAdded,
            DifferenceDirectionCodes.Request);

        Should.Throw<ArgumentOutOfRangeException>(() => _classifier.Classify(difference));
    }

    // Deprecation ve sunset bilgisi tasinmadigi icin her endpoint silme farkinin breaking oldugunu sabitler.
    [Fact]
    public void Endpoint_Removal_Without_Deprecation_Metadata_Should_Be_Breaking()
    {
        var difference = BuildDifference(
            DifferenceKindCodes.EndpointRemoved,
            DifferenceDirectionCodes.Endpoint);

        _classifier.Classify(difference).ShouldBe(DifferenceSeverityCodes.Breaking);
    }

    // Test girdisini urun tarafindaki kapali fark factory'sini kullanarak kurar.
    private static SpecDifferenceModel BuildDifference(string kindCode, string directionCode)
        => SpecDifferenceFactory.Modified(
            kindCode,
            directionCode,
            new FindingAddress(path: "/classification"),
            null,
            null);
}
