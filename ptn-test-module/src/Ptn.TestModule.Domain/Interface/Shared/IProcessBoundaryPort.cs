using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Shared;

namespace Ptn.TestModule.Interface.Shared;

// islevi: Manager'in kurdugu surec planini izole bir calisma klasorunde kosan capability'yi tanimlar.
// sistemdeki gorevi: Docker, dosya ve process ayrintisini tek sinir arkasinda toplar; lint ve kosum ayni sinirdan gecer.
/// <summary>
/// Bir surec planini calistirip ham sonucunu dondiren sozlesmedir.
/// </summary>
public interface IProcessBoundaryPort
{
    // Girdi dosyalarini yazar, sureci butceyle kosar ve istenen artefaktlari geri okur.
    /// <summary>Verilen plani calistirip cikis kodunu, akislarini ve artefaktlarini getirir.</summary>
    Task<ProcessExecutionOutcome> ExecuteAsync(
        ProcessExecutionPlan plan,
        CancellationToken cancellationToken = default);
}
