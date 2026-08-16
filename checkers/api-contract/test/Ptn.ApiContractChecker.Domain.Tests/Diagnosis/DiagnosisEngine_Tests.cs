using System.Text.Json;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Managers.Diagnosis;
using Ptn.ApiContractChecker.Managers.Diagnosis.Probes;
using Ptn.ApiContractChecker.Managers.Diagnosis.Rules;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Diagnosis;

// islevi: V1 rule, ranking, RuledOut gorunurlugu, report butcesi ve deterministiklik kabul olcutlerini kanitlar.
// sistemdeki gorevi: KBP-622 Faz 2 domain motorunun agsiz ve saf davranislarini regresyona karsi sabitler.
public class DiagnosisEngine_Tests
{
    [Fact]
    public async Task Required_Field_Finding_Should_Confirm_Without_Network_Probe()
    {
        var context = BuildContext();
        context.RelatedFindings.Add(new Finding(
            DifferenceKindCodes.NewRequiredRequestProperty,
            DifferenceSeverityCodes.Breaking,
            DifferenceDirectionCodes.Request,
            new FindingAddress(operationId: "createOrder", propertyPath: "customerId")));
        var rule = new RequiredRequestFieldCreatedRule();
        var probe = new ContractDriftFactProbe();
        var evidence = new List<ProbeEvidence>();
        foreach (var request in rule.RequiredProbes(context.Identity, context))
        {
            evidence.Add(await probe.RunAsync(request));
        }

        var result = rule.Assess(context.Identity, context, evidence);

        result.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Confirmed);
        evidence.ShouldAllBe(item => item.ProbeKindCode == ProbeKindCodes.ContractDriftFact);
    }

    [Theory]
    [InlineData(ProbeKindCodes.Facts.Absent, DiagnosisConfidenceCodes.Confirmed)]
    [InlineData(ProbeKindCodes.Facts.Present, DiagnosisConfidenceCodes.RuledOut)]
    public void Resource_Existence_Should_Confirm_Or_Rule_Out(string factCode, string confidence)
    {
        var context = BuildContext();
        context.Signal.ResourceUrl = "https://api.example/orders/1";
        context.Snapshot.Servers.Add("https://api.example");
        var rule = new ResourceNeverCreatedRule();

        var result = rule.Assess(context.Identity, context,
        [
            Evidence(rule.HypothesisKindCode, ProbeKindCodes.HeadResource, factCode)
        ]);

        result.ConfidenceCode.ShouldBe(confidence);
    }

    [Fact]
    public async Task Scope_Mismatch_Should_Be_Confirmed()
    {
        var context = BuildContext();
        context.Identity.ChallengeScheme = DiagnosisHttpConstants.Bearer;
        context.Identity.ChallengeScopes = ["orders.read"];
        context.Operation!.SecurityRequirements =
        [
            new SpecSecurityRequirementModel
            {
                Schemes = [new SpecSecuritySchemeModel { Name = "Bearer", Scopes = ["orders.write"] }]
            }
        ];
        var rule = new InsufficientScopeRule();
        var request = rule.RequiredProbes(context.Identity, context).Single();
        var proof = await new SpecFactProbe().RunAsync(request);

        var result = rule.Assess(context.Identity, context, [proof]);

        result.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Confirmed);
    }

    [Fact]
    public void Ranking_Should_Keep_RuledOut_And_All_Confirmed_Hypotheses()
    {
        var ranked = new HypothesisRankingManager().Rank(
        [
            Assessment("H-Z", DiagnosisConfidenceCodes.RuledOut, 100),
            Assessment("H-B", DiagnosisConfidenceCodes.Confirmed, 10),
            Assessment("H-A", DiagnosisConfidenceCodes.Confirmed, 10)
        ]);

        ranked.Select(item => item.HypothesisKindCode).ShouldBe(["H-A", "H-B", "H-Z"]);
        ranked.Count(item => item.ConfidenceCode == DiagnosisConfidenceCodes.Confirmed).ShouldBe(2);
    }

    [Fact]
    public void Report_Should_Trim_In_Order_And_Stay_Within_Four_Kilobytes()
    {
        var report = new DiagnosisReport
        {
            Detail = new string('d', 5000),
            NextChecks = Enumerable.Range(0, 10).Select(index => new string('n', 500 + index)).ToList(),
            Hypotheses = Enumerable.Range(0, 20).Select(index => new HypothesisAssessment
            {
                HypothesisKindCode = string.Concat("H-", index),
                ConfidenceCode = DiagnosisConfidenceCodes.Possible,
                Title = "title",
                Detail = new string('h', 1000),
                NextChecks = [new string('c', 800)],
                Evidence = [new ProbeEvidence { FactCode = "fact", ObservedValue = new string('e', 800) }]
            }).ToList()
        };

        report.TrimToBudget();

        report.MeasureUtf8Bytes().ShouldBeLessThanOrEqualTo(FailureSourceKindCodes.Report.MaxUtf8Bytes);
        report.Hypotheses.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Same_Assessments_Should_Produce_The_Same_Serialized_Order()
    {
        var input = new[]
        {
            Assessment("H-C", DiagnosisConfidenceCodes.Possible, 1),
            Assessment("H-A", DiagnosisConfidenceCodes.Confirmed, 1),
            Assessment("H-B", DiagnosisConfidenceCodes.RuledOut, 9)
        };
        var manager = new HypothesisRankingManager();

        var first = JsonSerializer.Serialize(manager.Rank(input));
        var second = JsonSerializer.Serialize(manager.Rank(input.Reverse()));

        first.ShouldBe(second);
    }

    // islevi: Rule testleri icin tek operasyonlu canli snapshot context'i kurar.
    private static ResolvedFailureContext BuildContext()
    {
        var operation = new SpecOperationModel { OperationId = "createOrder", Method = "POST", Path = "/orders" };
        var signal = new HttpFailureSignal { OperationId = operation.OperationId, Method = operation.Method, Path = operation.Path };
        var identity = new FailureIdentity { StatusClassCode = HttpStatusClassCodes.ClientError };
        return new ResolvedFailureContext
        {
            Snapshot = new SpecSnapshotModel { Operations = [operation] },
            Operation = operation,
            Signal = signal,
            Identity = identity
        };
    }

    // islevi: Tek rule'a ait probe kanitini test icin kisa yoldan kurar.
    private static ProbeEvidence Evidence(string hypothesisCode, string probeCode, string factCode)
        => new() { HypothesisKindCode = hypothesisCode, ProbeKindCode = probeCode, FactCode = factCode };

    // islevi: Ranking testleri icin kod, guven ve priority assessment'i kurar.
    private static HypothesisAssessment Assessment(string code, string confidence, int priority)
        => new() { HypothesisKindCode = code, ConfidenceCode = confidence, Priority = priority };
}
