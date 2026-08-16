using NSubstitute;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis.Identity;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Snapshots;
using Ptn.ApiContractChecker.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace Ptn.ApiContractChecker.Diagnosis;

// islevi: Kimlik dogrulama, body dislama, SSRF guard ve kismi probe butcesi invariantlarini kanitlar.
// sistemdeki gorevi: KBP-622 Faz 1 guvenlik sinirlarini hizli saf domain testleriyle sabitler.
public class DiagnosisSafety_Tests
{
    [Fact]
    public void Unknown_Challenge_Scheme_Should_Be_Dropped_And_Downgraded()
    {
        var resolver = new FailureIdentityExtractorResolver(
        [
            new ChallengeIdentityExtractor()
        ]);
        var signal = new HttpFailureSignal { StatusCode = 401 };
        signal.ResponseHeaders[DiagnosisHttpConstants.WwwAuthenticate] = "Bearer error=\"invalid_token\"";
        var snapshot = BuildSnapshot("ApiKey");

        var identity = resolver.Extract(signal, snapshot);

        identity.ChallengeScheme.ShouldBeNull();
        identity.IdentityConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Low);
    }

    [Fact]
    public void Failure_Signal_Should_Not_Expose_Raw_Body()
    {
        typeof(HttpFailureSignal).GetProperties()
            .ShouldNotContain(property => property.Name.Contains("Body", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Target_Outside_Snapshot_Servers_Should_Be_Rejected()
    {
        var request = new ProbeRequest
        {
            TargetUri = new Uri("https://attacker.example/orders/1"),
            AllowedServerUrls = ["https://api.example"],
            SpecPaths = ["/orders/{id}"]
        };

        var exception = Should.Throw<BusinessException>(() => new ProbeTargetGuard().EnsureAllowed(request));

        exception.Code.ShouldBe("ApiContractChecker.Diagnosis:UnsafeProbeTarget");
    }

    [Fact]
    public async Task Probe_Timeout_Should_Return_Partial_Evidence()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns(call =>
            ResolveSetting(call.ArgAt<string>(0)).ToString(System.Globalization.CultureInfo.InvariantCulture));
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);
        var manager = new ProbeBudgetManager(settings, clock,
        [
            new TestProbe("fast", false),
            new TestProbe("slow", true)
        ]);

        var result = await manager.RunAsync(
        [
            new ProbeRequest { ProbeKindCode = "fast" },
            new ProbeRequest { ProbeKindCode = "slow" }
        ]);

        result.Count.ShouldBe(1);
        result[0].FactCode.ShouldBe(ProbeKindCodes.Facts.Present);
    }

    // islevi: Test challenge scheme'ini tek operasyon security kataloguna yerlestirir.
    private static SpecSnapshotModel BuildSnapshot(string scheme)
        => new()
        {
            Operations =
            [
                new SpecOperationModel
                {
                    SecurityRequirements =
                    [
                        new SpecSecurityRequirementModel
                        {
                            Schemes = [new SpecSecuritySchemeModel { Name = scheme }]
                        }
                    ]
                }
            ]
        };

    // islevi: Probe butce testinin setting degerlerini ad bazinda sabitler.
    private static int ResolveSetting(string name)
        => name switch
        {
            var value when value == ApiContractCheckerSettings.Diagnosis.MaxProbeCount => 5,
            var value when value == ApiContractCheckerSettings.Diagnosis.MaxProbeDurationMs => 100,
            var value when value == ApiContractCheckerSettings.Diagnosis.ProbeTimeoutMs => 5,
            _ => 10
        };

    private sealed class TestProbe : IDiagnosisProbe
    {
        private readonly bool _slow;
        public string ProbeKindCode { get; }

        public TestProbe(string probeKindCode, bool slow)
        {
            ProbeKindCode = probeKindCode;
            _slow = slow;
        }

        // islevi: Hizli probe kanit dondurur, yavas probe cancellation gelene kadar bekler.
        public async Task<ProbeEvidence> RunAsync(
            ProbeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_slow)
            {
                await Task.Delay(100, cancellationToken);
            }

            return new ProbeEvidence { ProbeKindCode = ProbeKindCode, FactCode = ProbeKindCodes.Facts.Present };
        }
    }
}
