using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Probes;

// islevi: Tanili response header'inin varligini ve sinirli degerini olguya cevirir.
// sistemdeki gorevi: Rule siniflarini response header sozlugunun plumbing ayrintisindan ayirir.
public sealed class ResponseHeaderFactProbe : IDiagnosisProbe, ITransientDependency
{
    public string ProbeKindCode => ProbeKindCodes.ResponseHeaderFact;

    // islevi: Istenen header'i case-insensitive sinyal sozlugunden okur.
    public Task<ProbeEvidence> RunAsync(ProbeRequest request, CancellationToken cancellationToken = default)
    {
        string? value = null;
        var found = request.FactName != null &&
                    request.Context.Signal.ResponseHeaders.TryGetValue(request.FactName, out value);
        return Task.FromResult(new ProbeEvidence
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = found ? ProbeKindCodes.Facts.Present : ProbeKindCodes.Facts.Absent,
            ObservedValue = found ? value : null
        });
    }
}
