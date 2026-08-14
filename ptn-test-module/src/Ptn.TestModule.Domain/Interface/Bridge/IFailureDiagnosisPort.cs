using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: API ve database teshis checker'larini ayni kopru rapor sozlesmesiyle tanimlar.
// sistemdeki gorevi: Kanit motorunun iki checker DTO gramerini bilmeden kaynakli rapor almasini saglar.
public interface IFailureDiagnosisPort
{
    // Yapilandirilmis HTTP sinyalini API checker teshisine cevirir.
    Task<PtnDiagnosisReport> DiagnoseApiAsync(
        PtnDiagnosisRequest request,
        CancellationToken cancellationToken);

    // Yapilandirilmis assertion veya provider sinyalini database checker teshisine cevirir.
    Task<PtnDiagnosisReport> DiagnoseDatabaseAsync(
        PtnDiagnosisRequest request,
        CancellationToken cancellationToken);
}
