using Microsoft.Extensions.Localization;
using NSubstitute;
using Ptn.ApiContractChecker.Application.Mappers.Conformance;
using Ptn.ApiContractChecker.Application.Mappers.Diagnosis;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.Dtos.Correlation;
using Ptn.ApiContractChecker.Dtos.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Localization;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis.Identity;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Shouldly;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.ApiContractChecker.Correlation;

// islevi: Conformance ve diagnosis manager cikislarinin giris korelasyonunu Mapperly boyunca geri yansittigini kanitlar.
// sistemdeki gorevi: Echo davranisinin AppService elle atamasina veya konumsal eslesmeye baglanmasini engeller.
public class CorrelationEcho_Tests
{
    [Fact]
    public async Task Conformance_Results_Should_Echo_Their_Own_Correlation()
    {
        var mapper = new ConformanceMapper();
        var manager = BuildConformanceManager();
        var responseCorrelation = BuildCorrelation("response-step");
        var requestCorrelation = BuildCorrelation("request-step");

        var response = await manager.AssertResponseAsync(null, mapper.MapToRequest(new ResponseConformanceDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1",
            StatusCode = 200,
            Correlation = responseCorrelation
        }));
        var request = await manager.AssertRequestAsync(null, mapper.MapToRequest(new RequestConformanceDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1",
            Correlation = requestCorrelation
        }));

        AssertCorrelation(mapper.MapToDto(response).Correlation, responseCorrelation);
        AssertCorrelation(mapper.MapToDto(request).Correlation, requestCorrelation);
    }

    [Fact]
    public async Task Diagnosis_Report_Should_Echo_Its_Correlation()
    {
        var mapper = new DiagnosisMapper();
        var expected = BuildCorrelation("diagnosis-step");
        var signal = mapper.MapToSignal(new DiagnoseRequestDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/items/1",
            Correlation = expected
        });

        var report = await BuildDiagnosisManager().DiagnoseAsync(null, signal, []);

        AssertCorrelation(mapper.MapToDto(report).Correlation, expected);
    }

    [Fact]
    public async Task Results_Should_Keep_Missing_Correlation_Null()
    {
        var conformanceMapper = new ConformanceMapper();
        var conformance = await BuildConformanceManager().AssertResponseAsync(
            null,
            conformanceMapper.MapToRequest(new ResponseConformanceDto
            {
                SnapshotId = Guid.NewGuid(),
                Method = "GET",
                Path = "/items/1",
                StatusCode = 200
            }));
        var diagnosisMapper = new DiagnosisMapper();
        var diagnosis = await BuildDiagnosisManager().DiagnoseAsync(
            null,
            diagnosisMapper.MapToSignal(new DiagnoseRequestDto
            {
                SnapshotId = Guid.NewGuid(),
                Method = "GET",
                Path = "/items/1"
            }),
            []);

        conformanceMapper.MapToDto(conformance).Correlation.ShouldBeNull();
        diagnosisMapper.MapToDto(diagnosis).Correlation.ShouldBeNull();
    }

    // islevi: Conformance echo testleri icin dis I/O yapmayan manager bilesimini kurar.
    private static ResponseConformanceManager BuildConformanceManager()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return new ResponseConformanceManager(
            new OperationResolver(),
            Substitute.For<ISpecSchemaResolver>(),
            new ConformancePolicyResolver(),
            new ConformanceSettingsResolver(settings));
    }

    // islevi: Diagnosis echo testleri icin bos rule/probe koleksiyonlu deterministik manager bilesimini kurar.
    private static DiagnosisManager BuildDiagnosisManager()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        var localizer = Substitute.For<IStringLocalizer<ApiContractCheckerResource>>();
        localizer[Arg.Any<string>()].Returns(call =>
            new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);
        return new DiagnosisManager(
            Substitute.For<ISpecSchemaResolver>(),
            new FailureIdentityExtractorResolver([]),
            new FailureContextResolver(new OperationResolver()),
            new ProbeBudgetManager(settings, clock, []),
            new HypothesisRankingManager(),
            new DiagnosisReportNarrator(localizer),
            []);
    }

    // islevi: Test icin gecerli trace ile ayirt edilebilir tek adim korelasyonu kurar.
    private static CorrelationRefDto BuildCorrelation(string stepKey)
        => new() { TraceId = new string('a', 32), StepKey = stepKey };

    // islevi: Echo edilen korelasyonun iki alanini beklenen public degerlerle karsilastirir.
    private static void AssertCorrelation(CorrelationRefDto? actual, CorrelationRefDto expected)
    {
        actual.ShouldNotBeNull();
        actual.TraceId.ShouldBe(expected.TraceId);
        actual.StepKey.ShouldBe(expected.StepKey);
    }
}
