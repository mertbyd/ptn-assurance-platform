using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;

namespace Ptn.DatabaseChecker.Interface.Diagnosis;

// islevi: Katalogda dogrulanmis hedefte tek tur salt-okuma kanit sorgusunu calistirir.
// sistemdeki gorevi: ProbeBudgetManager'in adet/sure/cancellation butcesi altinda yazma veya serbest SQL yetenegi olmayan kanit ureticilerini toplar.
public interface IDiagnosisProbe
{
    string ProbeKindCode { get; }

    // islevi: Tek probe istegini mevcut repository omurgasinda calistirip redaction uygulanmis yapilandirilmis kanit dondurur.
    Task<ProbeEvidence> RunAsync(
        DatabaseConnection connection,
        ProbeRequest request,
        ValueRetentionPolicy retentionPolicy,
        CancellationToken cancellationToken = default);
}
