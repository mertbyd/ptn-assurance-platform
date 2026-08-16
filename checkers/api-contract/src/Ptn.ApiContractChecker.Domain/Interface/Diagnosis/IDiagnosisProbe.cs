using Ptn.ApiContractChecker.Models.Diagnosis;

namespace Ptn.ApiContractChecker.Interface.Diagnosis;

// islevi: Tek olgu veya safe HTTP kanit istegini calistiran probe sozlesmesidir.
// sistemdeki gorevi: Arayuzde POST, PUT, PATCH veya DELETE yetenegi acmadan butceli kanit ureticilerini toplar.
public interface IDiagnosisProbe
{
    string ProbeKindCode { get; }
    Task<ProbeEvidence> RunAsync(ProbeRequest request, CancellationToken cancellationToken = default);
}
