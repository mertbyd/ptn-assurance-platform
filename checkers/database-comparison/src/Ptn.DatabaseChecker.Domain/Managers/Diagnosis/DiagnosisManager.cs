using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Diagnosis;

// islevi: FailureSignal -> kimlik -> katalog -> hipotez -> probe -> siralama -> RFC rapor adimlarini sirayla orkestre eder.
// sistemdeki gorevi: Hata kodu veya provider karari tasimadan extractor, resolver, kurallar, butce ve ranking bilesenlerini tek use-case'te birlestirir.
public class DiagnosisManager : DomainService
{
    private readonly IEngineComponentResolver<IFailureIdentityExtractor> _extractorResolver;
    private readonly FailureContextResolver _contextResolver;
    private readonly ValueRetentionPolicyResolver _retentionPolicyResolver;
    private readonly ProbeBudgetManager _probeBudgetManager;
    private readonly HypothesisRankingManager _rankingManager;
    private readonly List<IDiagnosisRule> _rules;

    // islevi: Teshis akis adimlarini engine resolver, katalog resolver, rule koleksiyonu, probe butcesi ve siralama ile kurar.
    public DiagnosisManager(
        IEngineComponentResolver<IFailureIdentityExtractor> extractorResolver,
        FailureContextResolver contextResolver,
        ValueRetentionPolicyResolver retentionPolicyResolver,
        ProbeBudgetManager probeBudgetManager,
        HypothesisRankingManager rankingManager,
        IEnumerable<IDiagnosisRule> rules)
    {
        _extractorResolver = extractorResolver;
        _contextResolver = contextResolver;
        _retentionPolicyResolver = retentionPolicyResolver;
        _probeBudgetManager = probeBudgetManager;
        _rankingManager = rankingManager;
        _rules = rules.ToList();
    }

    // islevi: Dinamik teshisin yedi adimini karar mantigi eklemeden duz bir orkestrasyon halinde calistirir.
    public virtual async Task<DiagnosisReport> DiagnoseAsync(
        DatabaseConnection connection,
        FailureSignal signal,
        CancellationToken cancellationToken = default)
    {
        using var activity = DatabaseCheckerTelemetry.StartActivity(
            DatabaseCheckerTelemetryConstants.Activities.DiagnosisRun,
            connection.Engine.Code,
            connection.DatabaseName);
        var identity = ExtractIdentity(connection, signal);
        var retentionPolicy = await _retentionPolicyResolver.ResolveAsync();
        var context = await _contextResolver.ResolveAsync(
            connection, signal, identity, retentionPolicy, cancellationToken);
        var rules = SelectRules(identity, context);
        var requests = BuildProbeRequests(rules, identity, context);
        var evidence = await _probeBudgetManager.RunAsync(connection, requests, retentionPolicy, cancellationToken);
        var assessments = Assess(rules, identity, context, evidence);
        var maxHypotheses = await _probeBudgetManager.ResolveMaxHypothesesAsync();
        var report = _rankingManager.BuildReport(identity, context, assessments, maxHypotheses);
        report.Correlation = signal.Correlation;
        activity.SetProbeCount(evidence.Count);
        activity.SetOutcomeCode(
            report.Hypotheses.FirstOrDefault()?.HypothesisKindCode ??
            DatabaseCheckerTelemetryConstants.Outcomes.NoHypothesis);
        return report;
    }

    // islevi: Assertion icin baglanti, DB-exception icin sinyal engine koduyla conventional extractor'i secer.
    private FailureIdentity ExtractIdentity(DatabaseConnection connection, FailureSignal signal)
    {
        var engineCode = signal.DbException?.EngineCode ?? connection.Engine.Code;
        return _extractorResolver.Resolve(engineCode).Extract(signal);
    }

    // islevi: Olgu tabanli uygulanabilir kurallari oncelik ve tur koduyla kararli siralar.
    private List<IDiagnosisRule> SelectRules(
        FailureIdentity identity,
        ResolvedFailureContext context)
        => _rules
            .Where(rule => rule.AppliesTo(identity, context))
            .OrderByDescending(rule => rule.Priority)
            .ThenBy(rule => rule.HypothesisKindCode, System.StringComparer.Ordinal)
            .ToList();

    // islevi: Uygulanabilir kurallarin sinirli probe isteklerini tek listede toplar.
    private static List<ProbeRequest> BuildProbeRequests(
        List<IDiagnosisRule> rules,
        FailureIdentity identity,
        ResolvedFailureContext context)
        => rules.SelectMany(rule => rule.RequiredProbes(identity, context)).ToList();

    // islevi: Her uygulanabilir kurali ayni tamamlanmis kanit listesiyle bagimsiz degerlendirir.
    private static List<HypothesisAssessment> Assess(
        List<IDiagnosisRule> rules,
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<ProbeEvidence> evidence)
        => rules.Select(rule => rule.Assess(identity, context, evidence)).ToList();
}
