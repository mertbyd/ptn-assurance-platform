using Ptn.ApiContractChecker.Constants.Diagnosis;
using Ptn.ApiContractChecker.Interface.Diagnosis;
using Ptn.ApiContractChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Diagnosis.Probes;

// islevi: Yapilandirilmis problem error listesindeki en derin JSON Pointer adresini olguya cevirir.
// sistemdeki gorevi: Ham response govdesi veya regex kullanmadan schema ihlalini katalog adresine baglar.
public sealed class SchemaViolationLocationProbe : IDiagnosisProbe, ITransientDependency
{
    public string ProbeKindCode => ProbeKindCodes.SchemaViolationLocation;

    // islevi: En derin pointer'i deterministik uzunluk ve ordinal sirayla secer.
    public Task<ProbeEvidence> RunAsync(ProbeRequest request, CancellationToken cancellationToken = default)
    {
        var pointer = request.Context.Signal.ProblemErrors
            .Select(item => item.Pointer)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .OrderByDescending(item => item!.Length)
            .ThenBy(item => item, StringComparer.Ordinal)
            .FirstOrDefault();
        return Task.FromResult(new ProbeEvidence
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = pointer is null ? ProbeKindCodes.Facts.Absent : ProbeKindCodes.Facts.Present,
            ObservedValue = pointer
        });
    }
}
