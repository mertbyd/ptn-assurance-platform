using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis;

// islevi: Hipotezleri guven, oncelik ve kararli kod sirasina dizen saf fonksiyonu sunar.
// sistemdeki gorevi: Sonucu lokalize metin ve DI koleksiyon gelis sirasindan bagimsiz deterministik tutar.
public sealed class HypothesisRankingManager : ITransientDependency
{
    // islevi: Confirmed, Likely, Possible, RuledOut; sonra priority ve kod sirasini uygular.
    public List<HypothesisAssessment> Rank(IEnumerable<HypothesisAssessment> assessments)
        => assessments
            .OrderBy(item => DiagnosisConfidenceCodes.Rank(item.ConfidenceCode))
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.HypothesisKindCode, StringComparer.Ordinal)
            .ToList();
}
