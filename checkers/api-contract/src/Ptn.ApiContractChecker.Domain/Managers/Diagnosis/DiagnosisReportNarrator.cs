using Microsoft.Extensions.Localization;
using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Localization;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis;

// islevi: Sirali assessment'leri localization kaynagiyla RFC 9457 diagnosis raporuna anlatir.
// sistemdeki gorevi: Saf ranking'i metinden ayirir ve rapor butcesini tek cikis noktasinda uygular.
public sealed class DiagnosisReportNarrator : ITransientDependency
{
    private readonly IStringLocalizer<ApiContractCheckerResource> _localizer;

    public DiagnosisReportNarrator(IStringLocalizer<ApiContractCheckerResource> localizer)
    {
        _localizer = localizer;
    }

    // islevi: Hipotezleri sinirlar, lokalize eder, nextChecks'i toplar ve 4 KB tavana kirpar.
    public DiagnosisReport Build(
        FailureIdentity identity,
        ResolvedFailureContext context,
        List<HypothesisAssessment> assessments,
        int maxHypotheses)
    {
        var hypotheses = assessments.Take(maxHypotheses).ToList();
        hypotheses.ForEach(Localize);
        var report = new DiagnosisReport
        {
            Title = _localizer[DiagnosisLocalizationKeys.ReportTitle],
            Detail = _localizer[DiagnosisLocalizationKeys.ReportDetail],
            Identity = identity,
            Location = context.Location,
            Hypotheses = hypotheses,
            NextChecks = hypotheses.SelectMany(item => item.NextChecks)
                .Distinct(StringComparer.Ordinal)
                .Take(FailureSourceKindCodes.Report.MaxNextChecks)
                .ToList()
        };
        report.TrimToBudget();
        return report;
    }

    // islevi: Tek hipotezin baslik, detay ve elenmediyse next-check metnini kaynaktan doldurur.
    private void Localize(HypothesisAssessment assessment)
    {
        var prefix = string.Concat(DiagnosisLocalizationKeys.HypothesisPrefix, assessment.HypothesisKindCode);
        assessment.Title = _localizer[string.Concat(prefix, DiagnosisLocalizationKeys.TitleSuffix)];
        assessment.Detail = _localizer[string.Concat(prefix, DiagnosisLocalizationKeys.DetailSuffix)];
        if (assessment.ConfidenceCode != DiagnosisConfidenceCodes.RuledOut)
        {
            assessment.NextChecks.Add(_localizer[string.Concat(prefix, DiagnosisLocalizationKeys.NextCheckSuffix)]);
        }
    }
}
