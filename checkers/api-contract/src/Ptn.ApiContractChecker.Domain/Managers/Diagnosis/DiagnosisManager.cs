using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Diagnosis.Identity;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.Domain.Services;

namespace Ptn.ApiContractChecker.Managers.Diagnosis;

// islevi: Yakala, kimlikle, yerellestir, hipotez uret, kanit topla, sirala ve anlat adimlarini orkestre eder.
// sistemdeki gorevi: Hata veya hipotez turlerini bilmeden extractor, rule, probe butcesi ve narrator bilesenlerini birlestirir.
public class DiagnosisManager : DomainService
{
    private readonly ISpecSchemaResolver _schemaResolver;
    private readonly FailureIdentityExtractorResolver _identityResolver;
    private readonly FailureContextResolver _contextResolver;
    private readonly ProbeBudgetManager _probeBudgetManager;
    private readonly HypothesisRankingManager _rankingManager;
    private readonly DiagnosisReportNarrator _narrator;
    private readonly List<IDiagnosisRule> _rules;

    public DiagnosisManager(
        ISpecSchemaResolver schemaResolver,
        FailureIdentityExtractorResolver identityResolver,
        FailureContextResolver contextResolver,
        ProbeBudgetManager probeBudgetManager,
        HypothesisRankingManager rankingManager,
        DiagnosisReportNarrator narrator,
        IEnumerable<IDiagnosisRule> rules)
    {
        _schemaResolver = schemaResolver;
        _identityResolver = identityResolver;
        _contextResolver = contextResolver;
        _probeBudgetManager = probeBudgetManager;
        _rankingManager = rankingManager;
        _narrator = narrator;
        _rules = rules.ToList();
    }

    // islevi: Dinamik teshisin yedi adimini karar govdesi eklemeden sirayla calistirir.
    public async Task<DiagnosisReport> DiagnoseAsync(
        SpecSnapshot? snapshot,
        HttpFailureSignal signal,
        List<Finding> relatedFindings,
        CancellationToken cancellationToken = default)
    {
        var execution = await CaptureSignalAsync(snapshot, signal, relatedFindings);
        IdentifyFailure(execution);
        ResolveFailureContext(execution);
        GenerateHypotheses(execution);
        await CollectEvidenceAsync(execution, cancellationToken);
        RankHypotheses(execution);
        var report = await ExplainAsync(execution);
        report.Correlation = signal.Correlation;
        report.TrimToBudget();
        return report;
    }

    // islevi: Snapshot'in mevcut reader yuzeyinden modelini alir ve sinyal durumunu kurar.
    private async Task<DiagnosisExecution> CaptureSignalAsync(
        SpecSnapshot? snapshot,
        HttpFailureSignal signal,
        List<Finding> relatedFindings)
        => new()
        {
            Signal = signal,
            Snapshot = snapshot?.SpecContent == null
                ? new SpecSnapshotModel()
                : await _schemaResolver.GetSnapshotAsync(snapshot.SpecContent),
            RelatedFindings = relatedFindings.ToList()
        };

    // islevi: Yapilandirilmis extractor koleksiyonundan katalog-dogrulanmis failure identity uretir.
    private void IdentifyFailure(DiagnosisExecution execution)
        => execution.Identity = _identityResolver.Extract(execution.Signal, execution.Snapshot);

    // islevi: Kimligi snapshot operasyonu ve RelatedFindings ile tek context'e yerellestirir.
    private void ResolveFailureContext(DiagnosisExecution execution)
        => execution.Context = _contextResolver.Resolve(
            execution.Snapshot, execution.Signal, execution.Identity, execution.RelatedFindings);

    // islevi: Olguya uygulanabilen rule'lari priority ve kodla deterministik aday listesine cevirir.
    private void GenerateHypotheses(DiagnosisExecution execution)
        => execution.Rules = _rules
            .Where(rule => rule.AppliesTo(execution.Identity, execution.Context))
            .OrderByDescending(rule => rule.Priority)
            .ThenBy(rule => rule.HypothesisKindCode, StringComparer.Ordinal)
            .ToList();

    // islevi: Tum rule probe isteklerini tek butce altinda calistirip kismi kaniti korur.
    private async Task CollectEvidenceAsync(DiagnosisExecution execution, CancellationToken cancellationToken)
    {
        var requests = execution.Rules.SelectMany(rule =>
            rule.RequiredProbes(execution.Identity, execution.Context)).ToList();
        execution.Evidence = await _probeBudgetManager.RunAsync(requests, cancellationToken);
    }

    // islevi: Her rule'u ayni kanitla degerlendirip saf confidence/priority siralamasini uygular.
    private void RankHypotheses(DiagnosisExecution execution)
        => execution.Assessments = _rankingManager.Rank(execution.Rules.Select(rule =>
            rule.Assess(execution.Identity, execution.Context, execution.Evidence)));

    // islevi: Sirali assessment'leri tenant setting limitiyle lokalize ve butceli RFC rapora cevirir.
    private async Task<DiagnosisReport> ExplainAsync(DiagnosisExecution execution)
        => _narrator.Build(
            execution.Identity,
            execution.Context,
            execution.Assessments,
            await _probeBudgetManager.ResolveMaxHypothesesAsync());
}
