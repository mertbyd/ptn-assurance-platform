using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Probes;

// islevi: RelatedFindings icinde istenen difference kind kodunu aga cikmadan arar.
// sistemdeki gorevi: Sozlesme sapmasi hipotezlerini mevcut karsilastirma bulgusuna dogrudan baglar.
public sealed class ContractDriftFactProbe : IDiagnosisProbe, ITransientDependency
{
    public string ProbeKindCode => ProbeKindCodes.ContractDriftFact;

    // islevi: Finding kind eslesmesini kararli present/absent kanitina cevirir.
    public Task<ProbeEvidence> RunAsync(ProbeRequest request, CancellationToken cancellationToken = default)
    {
        var finding = request.Context.RelatedFindings.FirstOrDefault(item =>
            string.Equals(item.KindCode, request.FactName, StringComparison.Ordinal));
        return Task.FromResult(new ProbeEvidence
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = finding is null ? ProbeKindCodes.Facts.Absent : ProbeKindCodes.Facts.Present,
            ObservedValue = finding?.KindCode
        });
    }
}
