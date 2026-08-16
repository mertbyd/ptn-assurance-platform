using NSubstitute;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.Interface.Runs;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Managers.Runs;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Models.Runs;
using Shouldly;
using Xunit;
using Volo.Abp;

namespace Ptn.ApiContractChecker.Runs;

// islevi: Bulgu fingerprint kararliligini, None gizlilik damgasini ve runlar arasi change state kumelerini kanitlar.
// sistemdeki gorevi: Bakim ani kimliginin ham deger sizdirmeden kosudan bagimsiz kalmasini regresyona karsi korur.
public class FindingFingerprint_Tests
{
    // islevi: Ayni farkin iki kosuda ve ilgisiz baska bulgu yaninda ayni fingerprint'i urettigini kanitlar.
    [Fact]
    public void Same_Finding_Should_Have_Same_Fingerprint_Across_Runs()
    {
        var first = Retain(CreateFinding("old", "new"));
        var second = Retain(CreateFinding("old", "new"));
        var unrelated = Retain(new Finding(
            DifferenceKindCodes.EndpointAdded,
            DifferenceSeverityCodes.NonBreaking,
            DifferenceDirectionCodes.Endpoint,
            new FindingAddress(path: "/unrelated", httpMethod: "POST")));

        first.Fingerprint.ShouldBe(second.Fingerprint);
        unrelated.Fingerprint.ShouldNotBe(first.Fingerprint);
    }

    // islevi: None retention modunda degerin kendisi yerine tip ve varlik damgasinin hash'e girdigini kanitlar.
    [Fact]
    public void None_Retention_Should_Use_Type_And_Presence_Stamp()
    {
        var first = Retain(CreateFinding("1", "2"));
        var sameTypes = Retain(CreateFinding("10", "20"));
        var missingOld = Retain(CreateFinding(null, "20"));

        first.Fingerprint.ShouldBe(sameTypes.Fingerprint);
        first.Fingerprint.ShouldNotBe(missingOld.Fingerprint);
        first.OldValue.ShouldBeNull();
        first.NewValue.ShouldBeNull();
    }

    // islevi: Onceki run yok, tekrar eden, cozulmus ve eski null fingerprint durumlarini tek kume hesabinda kanitlar.
    [Fact]
    public void Change_State_Should_Classify_New_Known_Resolved_And_Unknown()
    {
        var manager = new FindingChangeStateManager(Substitute.For<IContractCheckRunRepository>());
        var firstRun = manager.Classify(["A", null], null);
        var secondRun = manager.Classify(["A", "B", null], ["A", "C"]);

        firstRun.Classify("A").ShouldBe(FindingChangeStateCodes.New);
        firstRun.Classify(null).ShouldBe(FindingChangeStateCodes.Unknown);
        secondRun.Classify("A").ShouldBe(FindingChangeStateCodes.Known);
        secondRun.Classify("B").ShouldBe(FindingChangeStateCodes.New);
        secondRun.ResolvedFingerprints.ShouldContain("C");
    }

    // islevi: Public grammar'in sekiz adres bilesenini entity normalizasyonundan sonra sabit sirada cerceveledigini kanitlar.
    [Fact]
    public void Address_Grammar_Should_Publish_Exact_Component_Order_And_Normalization()
    {
        var address = new FindingAddress(
            operationId: " getOrder ",
            httpMethod: " get ",
            path: " /orders/{id} ",
            schemaName: " Order ",
            propertyPath: " /id ",
            parameterName: " id ",
            responseStatus: " 200 ",
            mediaType: " Application/JSON ");
        var components = FindingAddressGrammar.BuildComponents(
            address.OperationId, address.HttpMethod, address.Path, address.SchemaName,
            address.PropertyPath, address.ParameterName, address.ResponseStatus, address.MediaType);

        FindingAddressGrammar.ComponentOrder.ShouldBe(
            "OperationId,HttpMethod,Path,SchemaName,PropertyPath,ParameterName,ResponseStatus,MediaType");
        FindingAddressGrammar.FingerprintComponentOrder.ShouldStartWith("KindCode,DirectionCode,");
        components.ShouldBe([
            "getOrder", "GET", "/orders/{id}", "Order", "/id", "id", "200", "application/json"
        ]);
        FindingAddressGrammar.Frame(components[1]).ShouldBe("3:GET");
        FindingAddressGrammar.Normalize(" ").ShouldBe("<empty>");
    }

    // islevi: Explicit referans hatasini ve SinceRunId varsayilan New seciminin explicit fingerprint ile kesismesini kanitlar.
    [Fact]
    public async Task Explicit_Reference_Should_Be_Required_And_Compose_With_Fingerprint_Filter()
    {
        const string fingerprintA = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        const string fingerprintB = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        var repository = Substitute.For<IContractCheckRunRepository>();
        repository.GetFindingFingerprintsAsync(Arg.Any<Guid>())
            .Returns([fingerprintA, fingerprintB, null]);
        repository.FindCompletedReferenceFindingFingerprintsAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((List<string?>?)null);
        var manager = new FindingChangeStateManager(repository);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.ClassifyAsync(Guid.NewGuid(), Guid.NewGuid()));
        exception.Code.ShouldBe(ContractCheckRunExceptionCodes.InvalidFindingReferenceRun);

        repository.FindCompletedReferenceFindingFingerprintsAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns([fingerprintA, null]);
        var referenceRunId = Guid.NewGuid();
        var classification = await manager.ClassifyAsync(Guid.NewGuid(), referenceRunId);
        var selection = manager.BuildSelection(
            classification, null, referenceRunId, [fingerprintB.ToLowerInvariant()]);

        selection.ShouldNotBeNull();
        selection.IncludeMissingFingerprint.ShouldBeFalse();
        selection.Fingerprints.ShouldBe([fingerprintB]);
        classification.Classify(null).ShouldBe(FindingChangeStateCodes.Unknown);
    }

    // islevi: Finding'i None retention politikasindan gecirip tek gercek kurulum yolundaki fingerprint'i dondurur.
    private static Finding Retain(Finding finding)
    {
        var manager = new FindingValueRetentionManager(
            new FindingValueRedactor(),
            new FindingFingerprintCalculator());
        return manager.Apply(
            new ContractCheckFindings([finding]),
            new ValueRetentionPolicy(ValueRetentionModeCodes.None, string.Empty)).Items.Single();
    }

    // islevi: Deger disindaki tum kimlik alanlari ayni olan bir response bulgusu kurar.
    private static Finding CreateFinding(string? oldValue, string? newValue)
        => new(
            DifferenceKindCodes.RequestPropertyTypeChanged,
            DifferenceSeverityCodes.Breaking,
            DifferenceDirectionCodes.Response,
            new FindingAddress(
                operationId: "getOrder",
                httpMethod: "GET",
                path: "/orders/{id}",
                schemaName: "Order",
                propertyPath: "/id",
                parameterName: "id",
                responseStatus: "200",
                mediaType: "application/json"),
            oldValue,
            newValue);
}
