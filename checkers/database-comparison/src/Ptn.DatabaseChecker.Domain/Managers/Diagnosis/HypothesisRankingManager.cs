using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Localization;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis;

// islevi: Hipotezleri guven/oncelik/kod sirasina dizer, lokalize eder ve RFC 9457 rapor butcesine indirger.
// sistemdeki gorevi: Kural hesaplamasini aciklama metni ile koleksiyon gelis sirasindan ayirip deterministik API cikisi uretir.
public sealed class HypothesisRankingManager : ITransientDependency
{
    private readonly IStringLocalizer<DatabaseCheckerResource> _localizer;

    // islevi: Siralama yoneticisini modulun localization kaynagiyla kurar.
    public HypothesisRankingManager(IStringLocalizer<DatabaseCheckerResource> localizer)
    {
        _localizer = localizer;
    }

    // islevi: Assessment listesini sinirlar, lokalize eder, RFC raporuna yerlestirir ve 4 KB tavana kirpar.
    public DiagnosisReport BuildReport(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<HypothesisAssessment> assessments,
        int maxHypotheses)
    {
        var hypotheses = Rank(assessments).Take(maxHypotheses).ToList();
        hypotheses.ForEach(Localize);
        var report = new DiagnosisReport
        {
            Title = _localizer["Diagnosis:Report:Title"],
            Detail = _localizer["Diagnosis:Report:Detail"],
            Identity = identity,
            Location = context.Location,
            Hypotheses = hypotheses,
            NextChecks = hypotheses.SelectMany(item => item.NextChecks).Distinct().Take(3).ToList()
        };
        report.TrimToBudget();
        return report;
    }

    // islevi: Confirmed-Likely-Possible-RuledOut, sonra oncelik ve kararli tur kodu sirasini uygular.
    private static IEnumerable<HypothesisAssessment> Rank(List<HypothesisAssessment> assessments)
        => assessments
            .OrderBy(item => DiagnosisConfidenceCodes.Rank(item.ConfidenceCode))
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.HypothesisKindCode, System.StringComparer.Ordinal);

    // islevi: Tek hipotezin baslik/detay ve gerekiyorsa next-check metinlerini localization kaynagindan doldurur.
    private void Localize(HypothesisAssessment assessment)
    {
        var prefix = $"Diagnosis:Hypothesis:{assessment.HypothesisKindCode}";
        assessment.Title = _localizer[$"{prefix}:Title"];
        assessment.Detail = _localizer[$"{prefix}:Detail"];
        if (assessment.ConfidenceCode != DiagnosisConfidenceCodes.RuledOut)
        {
            assessment.NextChecks.Add(_localizer[$"{prefix}:NextCheck"]);
        }
    }
}
